namespace OnkelMato.BlogEngine.Core.Configuration;

public class ImportExportConfiguration
{
    public bool ValidateCertificates { get; set; } = true;

    #region JWT settings

    public bool EnableJwtImport { get; set; } = true;
    public string[] JwtPublicCertificates { get; set; } = [];
    public bool EnableJwtExport { get; set; } = true;
    public string[] JwtPrivateCertificates { get; set; } = [];

    #endregion

    #region JSON settings

    public bool EnableJsonFileImport { get; set; } = true;
    public bool EnableJsonStringImport { get; set; } = true;
    public bool EnableJsonExport { get; set; } = true;

    #endregion

    #region Export to remote page. Automates JWT import 

    public bool EnableSyncToRemote { get; set; } = false;

    #endregion

    #region sync settings

    public bool EnableBlogExportSync { get; set; } = false;

    public bool EnableBlogImportSync { get; set; } = false;

    public string? BlogSyncSecret { get; set; }

    #endregion

    public bool AllowAnyExport => EnableJsonExport || EnableJwtExport || EnableSyncToRemote;

}
