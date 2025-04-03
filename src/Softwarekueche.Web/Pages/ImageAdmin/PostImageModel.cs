public class PostImageModel {
    public Guid UniqueId { get; set; }
    public string Name { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}