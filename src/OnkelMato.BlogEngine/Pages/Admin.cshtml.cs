using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OnkelMato.BlogEngine.Pages;

public class AdminModel(IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    public bool AllowNewPosts => postsConfiguration.CurrentValue.AllowBlogAdministration;
    public bool AcceptUnsignedImport => postsConfiguration.CurrentValue.AcceptUnsignedImport;
    public void OnGet()
    {
    }
}