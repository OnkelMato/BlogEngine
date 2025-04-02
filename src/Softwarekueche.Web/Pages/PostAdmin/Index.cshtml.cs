using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.PostAdmin
{
    public class IndexModel(SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

        public IList<PostAdminModel> Post { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Debug.WriteLine(_postsConfiguration.Value.AllowNewPosts);

            Post = await _context.Posts.Select(x => new PostAdminModel() { 
                UniqueId = x.UniqueId, MdContent = x.MdContent, Title = x.Title, UpdatedAt = x.UpdatedAt,
                IsPublished = x.IsPublished }).ToListAsync();

            if (!_postsConfiguration.Value.AllowNewPosts)
                return RedirectToPage("/Index");

            return Page();
        }
    }
}
