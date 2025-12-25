using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Pages;

public class ImageModel(BlogEngineReadRepository readRepository) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public async Task<ActionResult> OnGet()
    {
        var img = await readRepository.PostImageAsync(Id);
        return img is null
            ? File(Properties.Resources._1x1, "image/png") // maybe this is not the best idea
            : File(img.Image, img.ContentType);
    }
}