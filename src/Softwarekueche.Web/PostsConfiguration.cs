namespace Softwarekueche.Web;

public class PostsConfiguration
{
    public bool AllowNewPosts { get; set; } = false;
    public int PageSize { get; set; } = 12;
    public bool AcceptUnsignedImport { get; set; } = false;
    public string CertificateFile { get; set; } = "demo.crt";
}