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

    public IEnumerable<PostModel> Posts { get; set; } = null!;
    public bool AllowNewPosts => _postsConfiguration.CurrentValue.AllowAdministration;

    public bool AllowExport => importExportConfiguration.CurrentValue.AllowAnyExport;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!(_postsConfiguration.CurrentValue.AllowAdministration || importExportConfiguration.CurrentValue.AllowAnyExport))
            return RedirectToPage(RedirectUri ?? "/");

        var blog = repository.Blog();
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        Posts = (await repository.GetAllPosts()).Select(x=> x.ToModel());

        return Page();
    }
}