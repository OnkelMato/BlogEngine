using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class DeleteModel(BlogEngineContext context, IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
    private Post? _originalPost;

    [BindProperty]
    public PostAdminModel Post { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        if (id == null) { return NotFound("no id provided"); }

        _originalPost = await _context.Posts.FirstOrDefaultAsync(m => m.UniqueId == id && m.Blog == blog);

        if (_originalPost == null)
            return NotFound("Post not found");

        Post = new PostAdminModel()
        {
            MdContent = _originalPost.MdContent,
            MdPreview = _originalPost.MdPreview,
            PublishedAt = _originalPost.PublishedAt,
            Title = _originalPost.Title,
            UniqueId = _originalPost.UniqueId,
            UpdatedAt = _originalPost.UpdatedAt
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        var entity = _context.Posts.SingleOrDefault(x => x.UniqueId == id && x.Blog == blog);
        if (entity == null) { return NotFound($"Cannot find image with id {id}"); }

        _context.Posts.Remove(entity);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
