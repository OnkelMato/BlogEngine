using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class EditModel : PageModel
    {
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;

        public EditModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context)
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

            var postimage =  await _context.PostImages.FirstOrDefaultAsync(m => m.Id == id);
            if (postimage == null)
            {
                return NotFound();
            }
            PostImage = postimage;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PostImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostImageExists(PostImage.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PostImageExists(int id)
        {
            return _context.PostImages.Any(e => e.Id == id);
        }
    }
}
