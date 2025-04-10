using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class CreateModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration;

    public CreateModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

        Post = new PostAdminModel();
    }

    public IActionResult OnGet()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.Value.AllowNewPosts)
            return RedirectToPage("/Index");

        return Page();
    }

    [BindProperty]
    public PostAdminModel Post { get; set; } = null!;

    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.Value.AllowNewPosts)
            return RedirectToPage("/Index");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Posts.Add(new Post() { 
            CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, UniqueId = Guid.NewGuid(), MdContent = Post.MdContent, Title = Post.Title ,
            IsPublished = Post.IsPublished, MdPreview = Post.MdPreview});
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}