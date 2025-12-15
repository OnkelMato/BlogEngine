namespace OnkelMato.BlogEngine;

public class BlogConfiguration
{
    public Guid BlogUniqueId { get; set; } = Guid.Empty;
    public bool UseSingleBlog { get; set; } = false;
    public bool CreateBlogIfNotExist { get;set; } = false;
    public bool EnableBlogSelection { get;set; } = true;
    public string Language { get; set; } = "de-DE";
    public bool AllowAdministration { get; set; } = false;
    public int PostsPerPage { get; set; } = 12;
}