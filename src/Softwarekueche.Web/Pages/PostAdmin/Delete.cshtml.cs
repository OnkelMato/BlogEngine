using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.PostAdmin
{
    public class DeleteModel(SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
        private Post? _originalPost;

        [BindProperty]
        public PostAdminModel Post { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (!_postsConfiguration.Value.AllowNewPosts)
                return RedirectToPage("/Index");

            if (id == null)
            {
                return NotFound();
            }

            _originalPost = await _context.Posts.FirstOrDefaultAsync(m => m.UniqueId == id);

            if (_originalPost == null)
            {
                return NotFound();
            }
            else
            {
                // todo there must be something more elegant than this. Maybe a generic mapper?
                Post = new PostAdminModel() {MdContent = _originalPost.MdContent, Title = _originalPost.Title, UniqueId = _originalPost.UniqueId, UpdatedAt = _originalPost.UpdatedAt};
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(_originalPost!);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
