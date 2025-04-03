// create a model for the PostImage entity

internal class ImageModel {
    public Guid UniqueId { get; set; }
    public string Name { get; set; } = null!;
    public string Filename { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? AltText { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}