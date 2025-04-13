using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class IndexModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostImageModel> PostImage { get;set; } = null!;
    public bool AllowNewPosts => _postsConfiguration.CurrentValue.AllowBlogAdministration;

    public async Task<IActionResult> OnGetAsync()
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }
        
        PostImage = await _context.PostImages
            .Where(x=> x.Blog == blog)
            .Select(x=> new PostImageModel(){
                UniqueId = x.UniqueId,
                Name = x.Name,
                ContentType = x.ContentType,
                AltText = x.AltText,
                IsPublished = x.IsPublished,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync();

        return Page();
    }
}