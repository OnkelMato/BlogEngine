using System.ComponentModel.DataAnnotations;
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

    public SelectList EntityList { get; set; } = new(new[] { "Blog" });

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
            default:
                return RedirectToPage("./Admin");
        }
    }

    private void DoImportBlog(BlogExportModel importData, Blog blog)
    {
        var blogExport = JsonSerializer.Deserialize<BlogExportModel>(JsonDocument!) ?? new();

        // in case only a post or image was exported, the export values will be null.
        if (blogExport.IsFullExport)
        {
            blog.Title = blogExport.Title ?? blog.Title;
            blog.Description = blogExport.Description;
            blog.UpdatedAt = DateTime.Now;
            blog.CreatedAt = blogExport.CreatedAt;

            context.Blogs.Update(blog);
        }

        foreach (var postImage in blogExport.PostImages)
            DoImportPostImage(postImage, blog);

        foreach (var post in blogExport.Posts)
            DoImportPost(post, blog);
    }

    private void DoImportPost(PostExportModel postExport, Blog blog)
    {
        var postEntity = context.Posts.SingleOrDefault(x => x.UniqueId == postExport.UniqueId && x.Blog == blog);
        var headerImage = context.PostImages.SingleOrDefault(x => x.UniqueId == postExport.HeaderImage && x.Blog == blog);
        if (postEntity is null)
        {
            postEntity = new Post()
            {
                UniqueId = postExport.UniqueId,
                Blog = blog,
                HeaderImage = headerImage,
                MdContent = postExport.MdContent,
                Title = postExport.Title,
                UpdatedAt = DateTime.Now,
                ShowState = (ShowState)postExport.ShowState,
                MdPreview = postExport.MdPreview,
                Order = postExport.Order,
                CreatedAt = postExport.CreatedAt
            };
            context.Posts.Add(postEntity);
        }
        else
        {
            postEntity.Blog = blog;
            postEntity.HeaderImage = headerImage;
            postEntity.MdContent = postExport.MdContent;
            postEntity.Title = postExport.Title;
            postEntity.UpdatedAt = DateTime.Now;
            postEntity.ShowState = (ShowState)postExport.ShowState;
            postEntity.MdPreview = postExport.MdPreview;
            postEntity.CreatedAt = postExport.CreatedAt;
            context.Posts.Update(postEntity);
        }
    }

    private void DoImportPostImage(PostImageExportModel postImageExport, Blog blog)
    {
        var postImageEntity = context.PostImages.SingleOrDefault(x => x.UniqueId == postImageExport.UniqueId && x.Blog == blog);
        if (postImageEntity is null)
        {
            postImageEntity = new PostImage()
            {
                UniqueId = postImageExport.UniqueId,
                Blog = blog,
                AltText = postImageExport.AltText,
                ContentType = postImageExport.ContentType,
                Name = postImageExport.Name,
                Image = postImageExport.Image,
                IsPublished = postImageExport.IsPublished,
                UpdatedAt = postImageExport.UpdatedAt,
                CreatedAt = postImageExport.CreatedAt
            };
            context.PostImages.Add(postImageEntity);
        }
        else
        {
            postImageEntity.Blog = blog;
            postImageEntity.AltText = postImageExport.AltText;
            postImageEntity.ContentType = postImageExport.ContentType;
            postImageEntity.Name = postImageExport.Name;
            postImageEntity.Image = postImageExport.Image;
            postImageEntity.IsPublished = postImageExport.IsPublished;
            postImageEntity.UpdatedAt = postImageExport.UpdatedAt;
            postImageEntity.CreatedAt = postImageExport.CreatedAt;
            context.PostImages.Update(postImageEntity);
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