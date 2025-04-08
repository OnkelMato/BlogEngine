using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class IndexModel : PageModel
    {
        private readonly Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext _context;
        private readonly PostsConfiguration _postsConfiguration;

        public IndexModel(Softwarekueche.Web.Infrastructure.Data.SoftwarekuecheHomeContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        {
            _postsConfiguration = postsConfiguration.Value;
            _context = context;
        }

        public IList<PostImageModel> PostImage { get;set; } = null!;

        public async Task OnGetAsync()
        {
            // make sure it cannot be accessed if new posts are not allowed
            if (!_postsConfiguration.AllowNewPosts)
                RedirectToPage("/Index");

            PostImage = await _context.PostImages.Select(x=> new PostImageModel(){
                UniqueId = x.UniqueId,
                Name = x.Name,
                FileName = x.Filename,
                ContentType = x.ContentType,
                AltText = x.AltText,
                IsPublished = x.IsPublished,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync();
        }
    }
}
