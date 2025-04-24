using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Database;

public class PostTag
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UniqueId { get; set; }

    [Required]
    public Blog Blog { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = null!;
}