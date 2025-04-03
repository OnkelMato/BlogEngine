using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class DeleteModel : PageModel
    {
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;

        public DeleteModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postimage = await _context.PostImages.FindAsync(id);
            if (postimage != null)
            {
                PostImage = postimage;
                _context.PostImages.Remove(PostImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
