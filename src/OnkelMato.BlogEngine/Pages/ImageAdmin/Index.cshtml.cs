using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin
{
    public class IndexModel : PageModel
    {
        private readonly BlogEngineContext _context;
        private readonly PostsConfiguration _postsConfiguration;

        public IndexModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        {
            _postsConfiguration = postsConfiguration.Value;
            _context = context;
        }

        public IList<PostImageModel> PostImage { get;set; } = null!;
        public bool AllowNewPosts => _postsConfiguration.AllowNewPosts;

        public async Task OnGetAsync()
        {
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
