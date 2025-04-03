using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class IndexModel : PageModel
    {
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;

        public IndexModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context)
        {
            _context = context;
        }

        public IList<PostImage> PostImage { get;set; } = null!;

        public async Task OnGetAsync()
        {
            PostImage = await _context.PostImages.ToListAsync();
        }
    }
}
