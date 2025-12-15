namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class BlogAdminModel
{
    public Guid UniqueId { get; set; }
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string CSS { get; set; } = string.Empty;

    public IEnumerable<BlogItemModel> Blogs { get; set; } = [];

    public class BlogItemModel
    {
        public Guid BlogId { get; set; } = Guid.Empty;
        public string Title { get; set; } = "unknown blog";
    }
}