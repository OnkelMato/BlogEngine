using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.PostAdmin
{
    public class DetailsModel : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context;
        private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration;

        public DetailsModel(SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
        }

        public PostAdminModel Post { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (!_postsConfiguration.Value.AllowNewPosts)
                return RedirectToPage("/Index");

            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.UniqueId == id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                Post = new PostAdminModel()
                {
                    UniqueId = post.UniqueId,
                    Title = post.Title,
                    MdContent = post.MdContent,
                    UpdatedAt = post.UpdatedAt
                };
            }
            return Page();
        }
    }
}
