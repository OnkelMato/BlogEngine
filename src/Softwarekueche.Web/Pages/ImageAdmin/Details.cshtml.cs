using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class DetailsModel : PageModel
    {
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;

        public DetailsModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context)
        {
            _context = context;
        }

        public PostImage PostImage { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postimage = await _context.PostImages.FirstOrDefaultAsync(m => m.Id == id);
            if (postimage == null)
            {
                return NotFound();
            }
            else
            {
                PostImage = postimage;
            }
            return Page();
        }
    }
}
