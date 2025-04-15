using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class IndexModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IEnumerable<PostAdminModel> Posts { get; set; } = null!;
    public bool AllowNewPosts => _postsConfiguration.CurrentValue.AllowBlogAdministration;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        Posts = await _context.Posts
            .Where(x => x.Blog == blog)
            .OrderBy(x => x.Order)
            .Select(x => new PostAdminModel()
            {
                UniqueId = x.UniqueId,
                MdPreview = x.MdPreview,
                MdContent = x.MdContent,
                Title = x.Title,
                UpdatedAt = x.UpdatedAt,
                HeaderImage = x.HeaderImage == null ? null : x.HeaderImage.UniqueId,
                Order = x.Order,
                ShowState = x.ShowState.ToShowStateModel()
            })
            .ToListAsync();

        return Page();
    }
}