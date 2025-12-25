using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnkelMato.BlogEngine.Core.Database.Entity;

[Table("PostTag")]
public class PostTagDb
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the blog associated with this entity.
    /// Denormalized to allow faster queries.
    /// </summary>
    [Required]
    public BlogDb Blog { get; set; } = null!;

    [Required]
    public PostDb Post { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = null!;
}