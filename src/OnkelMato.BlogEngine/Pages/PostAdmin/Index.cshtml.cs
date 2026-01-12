using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class IndexModel(
    BlogEngineReadRepository repository,
    IOptionsMonitor<BlogConfiguration> postsConfiguration,
    IOptionsMonitor<ImportExportConfiguration> importExportConfiguration)
    : PageModel
{
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public string? RedirectUri { get; set; }

    public IEnumerable<PostModel> Posts { get; set; } = [];
    public bool AllowNewPosts => _postsConfiguration.CurrentValue.AllowAdministration;

    public bool AllowExport => importExportConfiguration.CurrentValue.AllowAnyExport;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!(_postsConfiguration.CurrentValue.AllowAdministration || importExportConfiguration.CurrentValue.AllowAnyExport))
            return RedirectToPage(RedirectUri ?? "/");

        try
        {
            Posts = (await repository.GetAllPosts()).Select(x => x.ToModel());
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = $"Error loading posts: {e.Message}";
            Posts = [];
        }

        return Page();
    }
}