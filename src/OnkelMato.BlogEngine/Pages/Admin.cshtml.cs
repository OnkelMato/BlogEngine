using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OnkelMato.BlogEngine.Pages
{
    public class AdminModel(IOptionsSnapshot<PostsConfiguration> postsConfiguration) : PageModel
    {
        private readonly PostsConfiguration _postsConfiguration = postsConfiguration.Value;
        public bool AllowNewPosts => _postsConfiguration.AllowNewPosts;
        public bool AcceptUnsignedImport => _postsConfiguration.AcceptUnsignedImport;
        public void OnGet()
        {
        }
    }
}
