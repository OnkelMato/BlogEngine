using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages
{
    public class ImageModel(SoftwarekuecheHomeContext context) : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; } = Guid.Empty;

        public ActionResult OnGet()
        {
            var img = _context.PostImages.Single(x => x.UniqueId == Id);

            return File(img.Image, img.ContentType);
        }


    }
}
