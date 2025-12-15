namespace OnkelMato.BlogEngine.Core.Model;

public class PostImage
{
    public Guid UniqueId { get; set; }
    public byte[] Image { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? AltText { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}