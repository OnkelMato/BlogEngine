using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class CreateModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration;

    public CreateModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

        Post = new PostAdminModel();
    }

    public IActionResult OnGet()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowBlogAdministration)
            return RedirectToPage("/Index");

        return Page();
    }

    [BindProperty]
    public PostAdminModel Post { get; set; } = null!;

    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowBlogAdministration)
            return RedirectToPage("/Index");

        if (!ModelState.IsValid)
            return Page();

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        var postHeaderImage = _context.PostImages.SingleOrDefault(x => x.UniqueId == Post.HeaderImage && x.Blog == blog);
        if (Post.HeaderImage != null && postHeaderImage is null)
        {
            ModelState.AddModelError(nameof(Post.HeaderImage), $"Cannot find header image {Post.HeaderImage} in post images of blog {blog.UniqueId}");
            return Page();
        }

        _context.Posts.Add(new Post()
        {
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UniqueId = Guid.NewGuid(),
            Blog = blog,
            MdContent = Post.MdContent,
            HeaderImage = postHeaderImage,
            Title = Post.Title,
            Order = Post.Order,
            ShowState = Post.ShowState.ToShowState(),
            MdPreview = Post.MdPreview
        });
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}