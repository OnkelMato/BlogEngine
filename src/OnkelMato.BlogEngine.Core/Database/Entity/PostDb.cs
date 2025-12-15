using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Core.Database.Entity;

[Index(nameof(Title), "BlogId", IsUnique = true, AllDescending = false)]
[Table("Post")]
public class PostDb
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UniqueId { get; set; }

    [Required]
    public BlogDb Blog { get; set; } = null!;

    public PostImageDb? HeaderImage { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    public string MdPreview { get; set; } = null!;

    [MaxLength(int.MaxValue)]
    public string? MdContent { get; set; }
    
    public ShowStateDb ShowState { get; set; } = ShowStateDb.None;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public DateTime PublishedAt { get; set; }

    [DefaultValue(1000)]
    public int Order { get; set; } = 1000;

    public List<PostTagDb> PostTags { get; set; } = [];
}