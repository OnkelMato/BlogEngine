namespace OnkelMato.BlogEngine.Core.Configuration;

public class ImportExportConfiguration
{
    public bool ValidateCertificates { get; set; } = true;

    public bool EnableJwtImport { get; set; } = true;
    public string[] JwtPublicCertificates { get; set; } = [];
    public bool EnableJwtExport { get; set; } = true;
    public string[] JwtPrivateCertificates { get; set; } = [];

    public bool EnableJsonFileImport { get; set; } = true;
    public bool EnableJsonStringImport { get; set; } = true;
    public bool EnableJsonExport { get; set; } = true;

    public bool EnableBlogSync { get; set; } = true;

    public bool AllowAnyExport => EnableJsonExport || EnableJwtExport;
}