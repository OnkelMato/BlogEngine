using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class IndexModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly PostsConfiguration _postsConfiguration;

    public IndexModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
    {
        _context = context;
        _postsConfiguration = postsConfiguration.Value;
    }

    public IEnumerable<PostAdminModel> Post { get; set; } = null!;
    public bool AllowNewPosts => _postsConfiguration.AllowBlogAdministration;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Post = await _context.Posts
            .OrderBy(x=> x.Order)
            .Select(x => new PostAdminModel() { 
                UniqueId = x.UniqueId,
                MdPreview = x.MdPreview,
                MdContent = x.MdContent,
                Title = x.Title,
                UpdatedAt = x.UpdatedAt,
                Order = x.Order,
                ShowState = x.ShowState.ToShowStateModel() })
            .ToListAsync();

        return Page();
    }
}