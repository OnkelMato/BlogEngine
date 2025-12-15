using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OnkelMato.BlogEngine.Pages;

public class AdminModel(
    IOptionsMonitor<BlogConfiguration> postsConfiguration,
    IOptionsMonitor<ImportExportConfiguration> imexConfiguration) : PageModel
{
    public bool AllowNewPosts => postsConfiguration.CurrentValue.AllowAdministration;
    public void OnGet()
    {
    }
}