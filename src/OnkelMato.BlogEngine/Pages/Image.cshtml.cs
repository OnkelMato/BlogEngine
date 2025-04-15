using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages;

public class ImageModel(BlogEngineContext context) : PageModel
{
    private readonly BlogEngineContext _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public ActionResult OnGet()
    {
        var img = _context.PostImages.SingleOrDefault(x => x.UniqueId == Id);
        return img is null 
            ? File(Properties.Resources._1x1, "image/png") // maybe this is not the best idea
            : File(img.Image, img.ContentType);
    }
}