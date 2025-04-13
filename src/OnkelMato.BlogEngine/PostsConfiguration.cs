namespace OnkelMato.BlogEngine;

public class PostsConfiguration
{
    public Guid? BlogUniqueId { get; set; } = null;
    public bool UseSingleBlog { get; set; } = false;
    public bool AllowBlogAdministration { get; set; } = false;
    public int PageSize { get; set; } = 12;
    public bool AcceptUnsignedImport { get; set; } = false;
    public string CertificateFile { get; set; } = "demo.crt";
}