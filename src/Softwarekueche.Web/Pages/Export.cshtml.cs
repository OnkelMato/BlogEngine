using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages
{
    public class ExportModel(SoftwarekuecheHomeContext context) : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; } = Guid.Empty;

        [BindProperty(SupportsGet = true)]
        public string? Entity { get; set; };

        public ActionResult OnGet()
        {
            if (Entity is null)
                throw new ArgumentException(nameof(Entity));

            Func<PostImage, bool> filter = Id == Guid.Empty 
                ? ((x) => true) 
                : ((x) => x.UniqueId == Id);
            
            var json = string.Empty;
            if (Entity.ToLower() == "posts")
            {
                var img = _context.PostImages.Where(filter);
                json = JsonSerializer.Serialize(img);
            } else if (Entity.ToLower() == "postimages") {
                var img = _context.PostImages.Where(filter);
                json = JsonSerializer.Serialize(img);
            }

            // we serialize a list so we can do batch import and export
            return File(json, "application/json");
        }
    }
}
