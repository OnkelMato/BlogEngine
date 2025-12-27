using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Core.Database.Entity;

[Index(nameof(Title), IsUnique = true)]
[Table("Blog")]
public class BlogDb
{
    [Key] public int Id { get; set; }
    [Required]
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(256)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public List<PostDb> Posts { get; set; } = [];
    
    public List<PostImageDb> PostImages { get; set; } = [];
    
    [MaxLength(10000)]
    
    public string? CSS { get; set; }
}