using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.PostAdmin
{
    public class IndexModel : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context;
        private readonly PostsConfiguration _postsConfiguration;

        public IndexModel(SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        {
            _context = context;
            _postsConfiguration = postsConfiguration.Value;
        }

        public IList<PostAdminModel> Post { get; set; } = null!;
        public bool AllowNewPosts => _postsConfiguration.AllowNewPosts;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Post = await _context.Posts.Select(x => new PostAdminModel() { 
                UniqueId = x.UniqueId, MdContent = x.MdContent, Title = x.Title, UpdatedAt = x.UpdatedAt,
                IsPublished = x.IsPublished }).ToListAsync();

            return Page();
        }
    }
}
