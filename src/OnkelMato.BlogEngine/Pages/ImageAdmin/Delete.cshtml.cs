using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class DeleteModel(BlogEngineContext context, IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : PageModel
{
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));

    [BindProperty]
    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        var postImage =  await _context.PostImages.SingleOrDefaultAsync(m => m.UniqueId == id && m.Blog == blog);
        if (postImage == null) { return NotFound($"Cannot find image with id {id}"); }

        PostImage = new PostImageModel() {
            UniqueId = postImage.UniqueId,
            Name = postImage.Name,
            ContentType = postImage.ContentType,
            AltText = postImage.AltText,
            IsPublished = postImage.IsPublished,
            CreatedAt = postImage.CreatedAt,
            UpdatedAt = postImage.UpdatedAt
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        var entity = _context.PostImages.SingleOrDefault(x => x.UniqueId == id && x.Blog == blog);
        if (entity == null) { return NotFound($"Cannot find image with id {id}"); }
        
        _context.PostImages.Remove(entity);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}