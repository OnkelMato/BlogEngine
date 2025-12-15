using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository.Model;

namespace OnkelMato.BlogEngine.Pages;

public class ExportModel(
    BlogEngineContext context,
    BlogEngineReadRepository readRepository,
    IOptionsMonitor<BlogConfiguration> postsConfiguration,
    IOptionsMonitor<ImportExportConfiguration> imexConfiguration,
    IBlogIdProvider blogIdProvider) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Entity { get; set; } = "Blog"; // Legacy: Blog, Posts, PostImages

    [BindProperty(SupportsGet = true)]
    public string? Type { get; set; } = "json"; // TODO replace with content type

    public async Task<ActionResult> OnGet()
    {
        if (Entity is null)
            throw new ArgumentException(nameof(Entity));

        var blogGuid = blogIdProvider.Id;
        var blog = readRepository.Blog();
        if (blog == null) { return NotFound($"Blog {blogGuid} not Found"); }

        BlogExportModel exportedEntities = null!;
        var filename = string.Empty;

        switch (Entity.ToLower())
        {
            // todo this could be a strategy pattern
            case "blog":
                {
                    // load full blog
                    blog = await readRepository.GetEntireBlog();
                    if (blog is null) throw new ArgumentException($"Blog {blogGuid} not Found");

                    exportedEntities = CreateBlogExport(blog);
                    filename = $"{blog!.Title}.{DateTime.Now:yyyyMMdd-hhmm}";

                    break;
                }
            case "posts":
                {
                    exportedEntities = CreatePostExport(Id, blog);
                    if (exportedEntities.Posts.Count == 0)
                        throw new ArgumentException($"No posts found for {Id}");

                    filename = Id == null
                        ? $"{blog!.Title}.GetEntireBlog.{DateTime.Now:yyyyMMdd-hhmm}"
                        : $"{exportedEntities.Posts.First().Title}.{DateTime.Now:yyyyMMdd-hhmm}";

                    break;
                }
            case "postimages":
                {
                    exportedEntities = CreatePostImageExport(Id, blog);
                    if (exportedEntities.PostImages.Count == 0)
                        throw new ArgumentException($"No posts found for {Id}");

                    filename = Id == null
                        ? $"{blog!.Title}.AllPostImages.{DateTime.Now:yyyyMMdd-hhmm}"
                        : $"{exportedEntities.PostImages.First().Name}.{DateTime.Now:yyyyMMdd-hhmm}";

                    break;
                }
            default:
                return BadRequest();
        }

        var exportAsJson = exportedEntities.AsJson();
        // todo make this selectable
        if (imexConfiguration.CurrentValue.JwtPublicCertificates.Length > 0 &&
            !string.IsNullOrWhiteSpace(imexConfiguration.CurrentValue.JwtPrivateCertificates[0])) // it requires a private key for signing
        {
            filename = filename + ".jwt.json";
            var token = GetTokenForJson(exportAsJson);
            return File(Encoding.UTF8.GetBytes(token), "application/json", filename);
        }

        filename = filename + ".json";
        return File(Encoding.UTF8.GetBytes(exportAsJson), "application/json", filename);
    }

    private BlogExportModel CreatePostImageExport(Guid? id, Blog blog)
    {
        var blogDb = context.Blogs.First(x => x.UniqueId == blog.UniqueId);
        Func<PostImageDb, bool> filter = (Id is null || Id == Guid.Empty)
            ? ((x) => x.Blog == blogDb)
            : ((x) => x.UniqueId == Id && x.Blog == blogDb);
        var img = context.PostImages
            .Where(filter)
            .Select(x => x.ToModel().ToPostImageExportModel()).ToArray();

        return new BlogExportModel()
        {
            IsFullExport = false,
            PostImages = img.ToList()
        };
    }

    private BlogExportModel CreatePostExport(Guid? id, Blog blog)
    {
        var blogDb = context.Blogs.First(x => x.UniqueId == blog.UniqueId);
        Func<PostDb, bool> filter = (Id is null || Id == Guid.Empty)
            ? ((x) => x.Blog == blogDb)
            : ((x) => x.UniqueId == Id && x.Blog == blogDb);
        var pst = context.Posts
            .Include(x => x.HeaderImage)
            .Where(filter)
            .Select(x => x.ToModel().ToPostExportModel()).ToArray();

        var img = new List<PostImage>();
        foreach (var post in pst)
        {
            if (post.HeaderImage == null) continue;
            var imgPost = context.PostImages.FirstOrDefault(x => x.UniqueId == post.HeaderImage);
            if (imgPost != null) img.Add(imgPost.ToModel());
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
                .Select(x => context.PostImages.FirstOrDefault(y => y.UniqueId == x && y.Blog == blogDb))
                .Where(x => x is not null).Select(x => x.ToModel());

            img.AddRange(imagesForPosts);
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
        // todo let user select the certificate -multiuser support :) 
        var cert = X509Certificate2.CreateFromPemFile(
            imexConfiguration.CurrentValue.JwtPublicCertificates[0], imexConfiguration.CurrentValue.JwtPrivateCertificates[0]);

        IJwtAlgorithm algorithm = new RS256Algorithm(cert);
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
        const string key = null; // not needed if algorithm is asymmetric

        return encoder.Encode(payload, key);
    }
}
