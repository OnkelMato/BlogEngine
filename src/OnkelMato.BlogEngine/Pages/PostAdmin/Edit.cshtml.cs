using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class EditModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly BlogEngineRepository _blogEngineRepository;
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration;

    public EditModel(
        BlogEngineContext context,
        BlogEngineRepository blogEngineRepository,
        IOptionsMonitor<BlogConfiguration> postsConfiguration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _blogEngineRepository = blogEngineRepository ?? throw new ArgumentNullException(nameof(blogEngineRepository));
        _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
    }

    [BindProperty]
    public PostAdminModel Post { get; set; } = null!;

    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public string? RedirectUri { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage(RedirectUri ?? "/Index");

        if (id == null)
        {
            return NotFound();
        }

        var post = await _blogEngineRepository.GetPostsWithHeaderImages(id.Value);
        if (post == null)
        {
            return NotFound();
        }
        Post = new PostAdminModel()
        {
            UniqueId = post.UniqueId,
            Title = post.Title,
            MdContent = post.MdContent,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt,
            Order = post.Order,
            ShowState = post.ShowState.ToShowStateModel(),
            HeaderImage = post.HeaderImage?.UniqueId,
            MdPreview = post.MdPreview
        };
        Post.UpdatedAt = DateTime.Now;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        var postHeaderImage = _context.PostImages.SingleOrDefault(x => x.UniqueId == Post.HeaderImage && x.Blog == blog);

        if (Post.HeaderImage != null && postHeaderImage is null)
        {
            ModelState.AddModelError("Post.HeaderImage", $"Cannot find header image {Post.HeaderImage} in images");
            return Page();
        }

        var dbPost = await _context.Posts.SingleAsync(m => m.UniqueId == Post.UniqueId && m.Blog == blog);

        dbPost.MdContent = Post.MdContent;
        dbPost.Title = Post.Title;
        dbPost.Order = Post.Order;
        dbPost.PublishedAt = Post.PublishedAt;
        dbPost.UpdatedAt = DateTime.Now;
        dbPost.HeaderImage = postHeaderImage;
        dbPost.ShowState = Post.ShowState.ToShowState();
        dbPost.MdPreview = Post.MdPreview;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(Post.UniqueId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        // currently not working. 
        //if (RedirectUri is not null)
        //    return RedirectToRoute(RedirectUri);

        return RedirectToPage("./Index");
    }

    private bool PostExists(Guid id)
    {
        return _context.Posts.Any(e => e.UniqueId == id && e.Blog.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
    }
}