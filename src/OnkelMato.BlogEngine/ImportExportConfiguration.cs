namespace OnkelMato.BlogEngine;

public class ImportExportConfiguration
{
    public bool ValidateCertificates { get; set; } = true;

    public bool EnableJwtImport { get; set; } = true;
    public string[] CertificateFile { get; set; } = ["demo.crt"];
    public bool EnableJwtExport { get; set; } = true;
    public string[] CertificateKeyFile { get; set; } = ["demo.key"];
    public string[] CertificateKeyPassword { get; set; } = [""];

    public bool EnableJsonFileImport { get; set; } = true;
    public bool EnableJsonStringImport { get; set; } = true;
    public bool EnableJsonExport { get; set; } = true;

    public bool EnableBlogSync { get; set; } = true;
}