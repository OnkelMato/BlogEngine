using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages;

public class ExportModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    private readonly BlogEngineContext _context = context;
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Entity { get; set; } = "Blog"; // Legacy: Blog, Posts, PostImages

    public async Task<ActionResult> OnGet()
    {
        if (Entity is null)
            throw new ArgumentException(nameof(Entity));

        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        switch (Entity.ToLower())
        {
            case "blog":
                {
                    // load full blog
                    blog = await context.Blogs
                        .Include(x=> x.Posts)
                        .Include(x=> x.PostImages)
                        .FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);

                    var filename = $"{blog.Title}.{DateTime.Now.ToString("yyyyMMdd-hhmm")}.json";
                    var export = CreateBlogExport(blog);
                    return File(Encoding.UTF8.GetBytes(export), "application/json", filename);
                }
            case "posts":
                {
                    Func<Post, bool> filter = Id == Guid.Empty
                        ? ((x) => x.Blog == blog)
                        : ((x) => x.UniqueId == Id && x.Blog == blog);

                    var pst = _context.Posts.Where(filter).ToArray();
                    var filename = pst.Length != 1
                        ? $"Posts.{DateTime.Now.ToShortDateString()}.json"
                        : $"{pst.First().Title}.json";
                    var json = JsonSerializer.Serialize(pst);
                    return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
                }
            case "postimages":
                {
                    Func<PostImage, bool> filter = Id == Guid.Empty
                        ? ((x) => x.Blog == blog)
                        : ((x) => x.UniqueId == Id && x.Blog == blog);

                    var img = _context.PostImages.Where(filter);
                    var filename = img.Count() != 1
                        ? $"PostImages.{DateTime.Now.ToShortDateString()}.json"
                        : $"{img.First().Name}.json";
                    var json = JsonSerializer.Serialize(img);
                    return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
                }
            default:
                return BadRequest();
        }
    }

    private string CreateBlogExport(Blog blog)
    {
        var posts = blog.Posts.Select(x => new BlogExportModel.PostExportModel()
        {
            CreatedAt = x.CreatedAt,
            MdContent = x.MdContent,
            MdPreview = x.MdPreview,
            ShowState = (int)x.ShowState,
            Title = x.Title,
            UpdatedAt = x.UpdatedAt,
            UniqueId = x.UniqueId,
            HeaderImage = x.HeaderImage?.UniqueId,
            Order = x.Order
        });

        var postImages = blog.PostImages.Select(x => new BlogExportModel.PostImageExportModel()
        {
            UniqueId = x.UniqueId,
            Image = x.Image,
            Name = x.Name,
            ContentType = x.ContentType,
            AltText = x.AltText,
            IsPublished = x.IsPublished,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        var export = new BlogExportModel()
        {
            Title = blog.Title,
            Description = blog.Description,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            Posts = posts.ToList(),
            PostImages = postImages.ToList(),
            IsFullExport = true
        };
        
        return JsonSerializer.Serialize(export);
    }
}