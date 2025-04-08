using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class DeleteModel : PageModel
    {
        private readonly PostsConfiguration _postsConfiguration;
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;

        public DeleteModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        {
            _postsConfiguration = postsConfiguration.Value;
            _context = context;
        }

        [BindProperty]
        public PostImageModel PostImage { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            // make sure it cannot be accessed if new posts are not allowed
            if (!_postsConfiguration.AllowNewPosts)
                RedirectToPage("/Index");

            var postimage =  await _context.PostImages.SingleAsync(m => m.UniqueId == id);
            if (postimage == null)
            {
                return NotFound();
            }

            PostImage = new PostImageModel() {
                UniqueId = postimage.UniqueId,
                Name = postimage.Name,
                FileName = postimage.Filename,
                ContentType = postimage.ContentType,
                AltText = postimage.AltText,
                IsPublished = postimage.IsPublished,
                CreatedAt = postimage.CreatedAt,
                UpdatedAt = postimage.UpdatedAt
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            // make sure it cannot be accessed if new posts are not allowed
            if (!_postsConfiguration.AllowNewPosts)
                RedirectToPage("/Index");

            var entity = _context.PostImages.Single(x => x.UniqueId == id);
            _context.PostImages.Remove(entity);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
