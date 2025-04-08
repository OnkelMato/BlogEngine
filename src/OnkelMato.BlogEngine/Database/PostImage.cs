using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Database;

public class PostImage {
    [Key]
    public int Id { get; set; }

    [Required] 
    public Guid UniqueId { get; set; }

    public byte[] Image { get; set; } = null!;

    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [MaxLength(256)]
    public string Filename { get; set; } = null!;

    [MaxLength(256)]
    public string ContentType { get; set; } = null!;

    [MaxLength(512)]
    public string? AltText { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
