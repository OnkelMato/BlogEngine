namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class BlogAdminModel
{
    public Guid UniqueId { get; set; }
    public string Title { get; set; } = null!;

    public string? Description { get; set; }
}