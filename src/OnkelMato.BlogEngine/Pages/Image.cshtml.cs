using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OnkelMato.BlogEngine.Pages;

public class ImageModel(BlogEngineRepository repository, IOptionsMonitor<BlogConfiguration> postsConfiguration, IBlogIdProvider blogId) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public async Task<ActionResult> OnGet()
    {
        var blog = repository.Blog;
        if (blog == null) { return NotFound($"Blog {blogId.Id} not Found"); }

        var img = repository.GetImage(Id);
        return img is null
            ? File(Properties.Resources._1x1, "image/png") // maybe this is not the best idea
            : File(img.Image, img.ContentType);
    }
}