using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Database;

[Index(nameof(Title), IsUnique = true)]
public class Blog
{
    [Key] public int Id { get; set; }
    [Required] public Guid UniqueId { get; set; }
    [MaxLength(256)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Post> Posts { get; set; } = [];
}