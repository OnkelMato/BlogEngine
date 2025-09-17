namespace OnkelMato.BlogEngine;

public class PostsConfiguration
{
    public Guid BlogUniqueId { get; set; } = Guid.Empty;
    public bool UseSingleBlog { get; set; } = false;
    public bool CreateBlogIfNotExist { get; set; } = false;
    public bool AllowBlogAdministration { get; set; } = false;
    public bool UseJwt { get; set; } = true;
    public int PageSize { get; set; } = 12;
    public bool AcceptUnsignedImport { get; set; } = false;
    public string CertificateFile { get; set; } = "demo.crt";
    public string CertificateKeyFile { get; set; } = "demo.key";
    public string CertificateKeyPassword { get; set; } = "";
}