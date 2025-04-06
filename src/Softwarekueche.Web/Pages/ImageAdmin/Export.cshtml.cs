using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace MyApp.Namespace
{
    public class ExportModel(SoftwarekuecheHomeContext context) : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; } = Guid.Empty;

        public ActionResult OnGet()
        {
            var img = _context.PostImages.Single(x => x.UniqueId == Id);
            var json = JsonSerializer.Serialize(img);

            return File(json, "application/json");
        }
    }
}
