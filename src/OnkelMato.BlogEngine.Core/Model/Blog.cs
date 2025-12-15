namespace OnkelMato.BlogEngine.Core.Model;

public class Blog
{
    public Guid UniqueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? CSS { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Post> Posts { get; set; } = [];
    public List<PostImage> PostImages { get; set; } = [];
    public List<PostTag> PostTags { get; set; } = [];
}