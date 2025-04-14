using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine;
using OnkelMato.BlogEngine.Database;
using OnkelMato.BlogEngine.Pages.PostAdmin;

namespace Softwarekueche.Web.ViewComponents;

public class BlogMenuViewComponent(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : ViewComponent
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public List<PostAdminModel> MenuPosts { get; set; } = [];

    public async Task<IViewComponentResult> InvokeAsync(int maxPriority, bool isDone)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return View(this); }

        MenuPosts = await _context.Posts
            .Where(x => x.Blog == blog && (x.ShowState == ShowState.Menu || x.ShowState == ShowState.BlogAndMenu))
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

        return View(this);
    }
}