using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;

namespace OnkelMato.BlogEngine.Pages;

public class AdminModel(
    IOptionsMonitor<BlogConfiguration> blogConfiguration,
    IOptionsMonitor<ImportExportConfiguration> importExportConfiguration) : PageModel
{
    public bool AllowNewPosts => blogConfiguration.CurrentValue.AllowAdministration;

    public bool AllowUnsignedImport => !importExportConfiguration.CurrentValue.ValidateCertificates;

    public bool AllowExport => importExportConfiguration.CurrentValue.AllowAnyExport;

    public bool JwtCertificateMissing => importExportConfiguration.CurrentValue is { EnableJsonExport: true, JwtPrivateCertificates.Length: 0 };

    public void OnGet()
    {
    }
}