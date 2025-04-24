using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages;

public class ImageModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    private readonly BlogEngineContext _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public async Task< ActionResult> OnGet()
    {
        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        var img = _context.PostImages.SingleOrDefault(x => x.UniqueId == Id && x.Blog == blog );
        return img is null 
            ? File(Properties.Resources._1x1, "image/png") // maybe this is not the best idea
            : File(img.Image, img.ContentType);
    }
}