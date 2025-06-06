﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnkelMato.BlogEngine.Database;

[Index(nameof(Title), "BlogId", IsUnique = true, AllDescending = false)]
public class Post
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UniqueId { get; set; }

    [Required]
    public Blog Blog { get; set; } = null!;

    public PostImage? HeaderImage { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    public string MdPreview { get; set; } = null!;

    [MaxLength(int.MaxValue)]
    public string? MdContent { get; set; }
    
    public ShowState ShowState { get; set; } = ShowState.None;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public DateTime PublishedAt { get; set; }

    [DefaultValue(1000)]
    public int Order { get; set; } = 1000;

    public List<PostTag> PostTags { get; set; } = [];
}