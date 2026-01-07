namespace OnkelMato.BlogEngine.Core.Configuration;

public class BlogConfiguration
{
    public Guid? BlogUniqueId { get; set; }
    public bool EnableBlogSelection { get; set; } = true;
    public string Language { get; set; } = "de-DE";
    public bool AllowAdministration { get; set; } = false;
    public bool AllowBlogCreation { get; set; } = false;
    public bool AllowBlogDeletion { get; set; } = false;
    public int PostsPerPage { get; set; } = 12;
}