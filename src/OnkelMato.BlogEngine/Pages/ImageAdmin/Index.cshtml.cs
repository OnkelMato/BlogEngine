using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class IndexModel(
    BlogEngineReadRepository repository,
    IOptionsMonitor<BlogConfiguration> postsConfiguration,
    IOptionsMonitor<ImportExportConfiguration> importExportConfiguration)
    : PageModel
{
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public string? RedirectUri { get; set; }

    public IList<PostImageModel> PostImage { get;set; } = null!;
    public bool AllowNewPosts => _postsConfiguration.CurrentValue.AllowAdministration;
    public bool AllowExport => importExportConfiguration.CurrentValue.AllowAnyExport;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!(_postsConfiguration.CurrentValue.AllowAdministration || importExportConfiguration.CurrentValue.AllowAnyExport))
            return RedirectToPage(RedirectUri ?? "/");

        var blog = repository.Blog();
        
        PostImage = repository.GetAllImages().Select(x=> x.ToModel()).ToList();

        return Page();
    }
}