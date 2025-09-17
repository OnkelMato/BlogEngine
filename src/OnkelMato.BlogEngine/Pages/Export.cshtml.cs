using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace OnkelMato.BlogEngine.Pages;

public class ExportModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Entity { get; set; } = "Blog"; // Legacy: Blog, Posts, PostImages

    public async Task<ActionResult> OnGet()
    {
        if (Entity is null)
            throw new ArgumentException(nameof(Entity));

        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        switch (Entity.ToLower())
        {
            case "blog":
                {
                    // load full blog
                    blog = await context.Blogs
                        .Include(x => x.Posts).ThenInclude(x => x.HeaderImage)
                        .Include(x => x.PostImages)
                        .FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
                    if (blog is null) throw new ArgumentException($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found");

                    var blogExport = CreateBlogExport(blog);
                    var export = blogExport.AsJson();
                    if (postsConfiguration.CurrentValue.UseJwt && postsConfiguration.CurrentValue.CertificateKeyFile != "")
                        export = GetTokenForJson(blogExport);

                    var filename = $"{blog!.Title}.{DateTime.Now:yyyyMMdd-hhmm}.json";
                    return File(Encoding.UTF8.GetBytes(export), "application/json", filename);
                }
            case "posts":
                {
                    var export = CreatePostExport(Id, blog);
                    if (export.Posts.Count == 0)
                        throw new ArgumentException($"No posts found for {Id}");

                    var filename = Id == null
                        ? $"{blog!.Title}.AllPosts.{DateTime.Now:yyyyMMdd-hhmm}.json"
                        : $"{export.Posts.First().Title}.{DateTime.Now:yyyyMMdd-hhmm}.json";
                    return File(Encoding.UTF8.GetBytes(export.AsJson()), "application/json", filename);
                }
            case "postimages":
                {
                    var export = CreatePostImageExport(Id, blog);
                    if (export.PostImages.Count == 0)
                        throw new ArgumentException($"No posts found for {Id}");

                    var filename = Id == null
                        ? $"{blog!.Title}.AllPostImages.{DateTime.Now:yyyyMMdd-hhmm}.json"
                        : $"{export.PostImages.First().Name}.{DateTime.Now:yyyyMMdd-hhmm}.json";
                    return File(Encoding.UTF8.GetBytes(export.AsJson()), "application/json", filename);
                }
            default:
                return BadRequest();
        }
    }

    private BlogExportModel CreatePostImageExport(Guid? id, Blog blog)
    {
        Func<PostImage, bool> filter = (Id is null || Id == Guid.Empty)
            ? ((x) => x.Blog == blog)
            : ((x) => x.UniqueId == Id && x.Blog == blog);
        var img = context.PostImages
            .Where(filter)
            .Select(x => x.ToPostImageExportModel()).ToArray();

        return new BlogExportModel()
        {
            IsFullExport = false,
            PostImages = img.ToList()
        };
    }

    private BlogExportModel CreatePostExport(Guid? id, Blog blog)
    {
        Func<Post, bool> filter = (Id is null || Id == Guid.Empty)
            ? ((x) => x.Blog == blog)
            : ((x) => x.UniqueId == Id && x.Blog == blog);
        var pst = context.Posts
            .Include(x => x.HeaderImage)
            .Where(filter)
            .Select(x => x.ToPostExportModel()).ToArray();

        var img = new List<PostImage>();
        foreach (var post in pst)
        {
            if (post.HeaderImage == null) continue;
            var imgPost = context.PostImages.FirstOrDefault(x => x.UniqueId == post.HeaderImage);
            if (imgPost != null) img.Add(imgPost);
        }

        var regex = new Regex(@"([Ii][Mm][Aa][Gg][Ee]\?[Ii][Dd]=([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}))");
        var regexPrefix = "image?id=".Length;
        foreach (var model in pst)
        {
            var imagesForPosts = regex
                .Matches(model.MdContent ?? string.Empty)
                .Concat(regex.Matches(model.MdPreview))
                .Select(x => x.Value.Substring(regexPrefix))
                .Distinct()
                .Select(Guid.Parse)
                .Select(x => context.PostImages.FirstOrDefault(y => y.UniqueId == x && y.Blog == blog))
                .Where(x => x is not null);

            img.AddRange(imagesForPosts!);
        }

        var imageExportList = img.Distinct().Select(x => x.ToPostImageExportModel());
        return new BlogExportModel()
        {
            IsFullExport = false,
            Posts = pst.ToList(),
            PostImages = imageExportList.ToList()
        };
    }

    private BlogExportModel CreateBlogExport(Blog blog)
    {
        var posts = blog.Posts.Select(x => x.ToPostExportModel());

        var postImages = blog.PostImages.Select(x => x.ToPostImageExportModel());

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

        return export;
    }

    private string GetTokenForJson(object payload)
    {
        var cert = X509Certificate2.CreateFromPemFile(postsConfiguration.CurrentValue.CertificateFile, postsConfiguration.CurrentValue.CertificateKeyFile);

        IJwtAlgorithm algorithm = new RS256Algorithm(cert);
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        const string key = null; // not needed if algorithm is asymmetric

        return encoder.Encode(payload, key);
    }
}