using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;
using static OnkelMato.BlogEngine.Pages.BlogExportModel;

namespace OnkelMato.BlogEngine.Pages;

public class ImportModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    public string SignaturePublicKey => postsConfiguration.CurrentValue.CertificateFile;
    public bool AcceptUnsignedImport => postsConfiguration.CurrentValue.AcceptUnsignedImport;

    [BindProperty(SupportsGet = true)]
    public bool UseTextarea { get; set; }

    [BindProperty]
    [Display(Name = "Json with Data")]
    public string? JsonDocument { get; set; }
    [BindProperty]
    [Display(Name = "Json with Data")]
    public IFormFile? JsonDocumentFile { get; set; } = null!;
    [BindProperty]
    [Display(Name = "Signature for Json")]
    public string? Signature { get; set; }
    [BindProperty]
    [Display(Name = "Signature for Json")]
    public IFormFile? SignatureFile { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Import Type")]
    public string Entity { get; set; } = null!;

    // todo remotve this
    public SelectList EntityList { get; set; } = new(new[] { "Posts", "PostImages", "Blog" });

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        if (JsonDocumentFile is not null)
        {
            using var stream = new MemoryStream();
            await JsonDocumentFile.CopyToAsync(stream);
            stream.Position = 0;
            JsonDocument = await new StreamReader(stream).ReadToEndAsync();
        }

        if (SignatureFile is not null)
        {
            using var stream = new MemoryStream();
            await SignatureFile.CopyToAsync(stream);
            stream.Position = 0;
            Signature = await new StreamReader(stream).ReadToEndAsync();
        }

        if (string.IsNullOrEmpty(Signature) && !postsConfiguration.CurrentValue.AcceptUnsignedImport)
        {
            ModelState.AddModelError(nameof(Signature), "Signature is required because 'AcceptUnsignedImport' is set to false.");
            ModelState.AddModelError(nameof(SignatureFile), "Signature is required because 'AcceptUnsignedImport' is set to false.");
            return Page();
        }

        if (Signature is not null)
        {
            var cert = new X509Certificate2(postsConfiguration.CurrentValue.CertificateFile);
            if (!Verify(JsonDocument!, Signature, cert))
            {
                ModelState.AddModelError(nameof(Signature), "Signature is invalid.");
                ModelState.AddModelError(nameof(SignatureFile), "Signature is invalid.");
                return Page();
            }
        }

        // this is perfect for strategy pattern
        switch (Entity.ToLower())
        {
            case "blog":
                {
                    var importData = JsonSerializer.Deserialize<BlogExportModel>(JsonDocument!) ?? new();
                    DoImportBlog(importData, blog);

                    await context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
            case "posts":
                {
                    var entities = JsonSerializer.Deserialize<IEnumerable<Post>>(JsonDocument!) ?? [];
                    foreach (var entity in entities)
                        DoImportPost(entity, blog);

                    await context.SaveChangesAsync();
                    return RedirectToPage("./PostAdmin/Index");
                }
            case "postimages":
                {
                    var entities = JsonSerializer.Deserialize<IEnumerable<PostImage>>(JsonDocument!) ?? [];
                    foreach (var entity in entities)
                        DoImportPostImage(entity, blog);

                    await context.SaveChangesAsync();
                    return RedirectToPage("./ImageAdmin/Index");
                }
            default:
                return RedirectToPage("./Admin");
        }
    }

    private void DoImportPostImage(PostImage entity, Blog blog)
    {
        var postImage = context.PostImages.SingleOrDefault(x => x.UniqueId == entity.UniqueId);
        if (postImage is null)
        {
            entity.Id = 0;
            entity.Blog = blog;
            context.PostImages.Add(entity);
        }
        else
        {
            postImage.AltText = entity.AltText;
            postImage.Blog = blog;
            postImage.ContentType = entity.ContentType;
            postImage.Name = entity.Name;
            postImage.Image = entity.Image;
            postImage.UpdatedAt = DateTime.Now;
            postImage.IsPublished = entity.IsPublished;
            postImage.UpdatedAt = entity.UpdatedAt;
            postImage.CreatedAt = entity.CreatedAt;
        }
    }

    private void DoImportPost(Post entity, Blog blog)
    {
        var post = context.Posts.SingleOrDefault(x => x.UniqueId == entity.UniqueId && x.Blog == blog);
        if (post is null)
        {
            entity.Id = 0;
            entity.Blog = blog;
            context.Posts.Add(entity);
        }
        else
        {
            post.Blog = blog;
            post.HeaderImage = entity.HeaderImage;
            post.MdContent = entity.MdContent;
            post.Title = entity.Title;
            post.UpdatedAt = DateTime.Now;
            post.ShowState = entity.ShowState;
            post.MdPreview = entity.MdPreview;
            post.CreatedAt = entity.CreatedAt;
        }
    }

    private void DoImportBlog(BlogExportModel importData, Blog blog)
    {
        var blogExport = JsonSerializer.Deserialize<BlogExportModel>(JsonDocument!) ?? new();
        blog.Title = blogExport.Title ?? blog.Title;
        blog.Description = blogExport.Description;
        blog.UpdatedAt = DateTime.Now;
        context.Blogs.Update(blog);

        foreach (var post in blogExport.Posts)
            DoImportPost(post, blog);

        foreach (var postImage in blogExport.PostImages)
            DoImportPostImage(postImage, blog);
    }

    private void DoImportPost(PostExportModel entity, Blog blog)
    {
        var post = context.Posts.SingleOrDefault(x => x.UniqueId == entity.UniqueId && x.Blog == blog);
        var headerImage = context.PostImages.SingleOrDefault(x => x.UniqueId == entity.HeaderImage && x.Blog == blog);
        if (post is null)
        {
            post = new Post()
            {
                UniqueId = entity.UniqueId,
                Blog = blog,
                HeaderImage = headerImage,
                MdContent = entity.MdContent,
                Title = entity.Title,
                UpdatedAt = DateTime.Now,
                ShowState = (ShowState)entity.ShowState,
                MdPreview = entity.MdPreview,
                Order = entity.Order,
                CreatedAt = entity.CreatedAt

            };
            context.Posts.Add(post);
        }
        else
        {
            post.Blog = blog;
            post.HeaderImage = headerImage;
            post.MdContent = entity.MdContent;
            post.Title = entity.Title;
            post.UpdatedAt = DateTime.Now;
            post.ShowState = (ShowState)entity.ShowState;
            post.MdPreview = entity.MdPreview;
            post.CreatedAt = entity.CreatedAt;
        }
    }

    private void DoImportPostImage(PostImageExportModel postImage, Blog blog)
    {
        var postImageEntity = context.PostImages.SingleOrDefault(x => x.UniqueId == postImage.UniqueId && x.Blog == blog);
        if (postImageEntity is null)
        {
            postImageEntity = new PostImage()
            {
                UniqueId = postImage.UniqueId,
                Blog = blog,
                AltText = postImage.AltText,
                ContentType = postImage.ContentType,
                Name = postImage.Name,
                Image = postImage.Image,
                IsPublished = postImage.IsPublished,
                CreatedAt = postImage.CreatedAt
            };
            context.PostImages.Add(postImageEntity);
        }
        else
        {
            postImageEntity.Blog = blog;
            postImageEntity.AltText = postImage.AltText;
            postImageEntity.ContentType = postImage.ContentType;
            postImageEntity.Name = postImage.Name;
            postImageEntity.Image = postImage.Image;
            postImageEntity.IsPublished = postImage.IsPublished;
            postImageEntity.CreatedAt = postImage.CreatedAt;
        }
    }

    public static bool Verify(string data, string signature, X509Certificate2 serverCert)
    {
        try
        {
            using var publicKey = serverCert.GetRSAPublicKey();
            var dataByteArray = Encoding.UTF8.GetBytes(data);
            var signatureByteArray = Convert.FromBase64String(signature);

            return publicKey!.VerifyData(
                data: dataByteArray,
                signature: signatureByteArray,
                hashAlgorithm: HashAlgorithmName.SHA256,
                padding: RSASignaturePadding.Pkcs1);
        }
        catch (Exception)
        {
            return false;
        }
    }
}