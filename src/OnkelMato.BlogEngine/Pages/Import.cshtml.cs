using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages
{
    public class ImportModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration) : PageModel
    {
        public string SignaturePublicKey => postsConfiguration.Value.CertificateFile;

        [BindProperty(SupportsGet = true)]
        public bool UseTextarea { get; set; }

        [BindProperty]
        public string? JsonDocument { get; set; }
        [BindProperty]
        public IFormFile? JsonDocumentFile { get; set; } = null!;
        [BindProperty]
        public string? Signature { get; set; }
        [BindProperty]
        public IFormFile? SignatureFile { get; set; } = null!;
     
        [BindProperty(SupportsGet = true)]
        public string Entity { get; set; } = null!;

        public SelectList EntityList { get; set; } = new(new[] { "Posts", "PostImages" });

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

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

            if (string.IsNullOrEmpty(Signature) && !postsConfiguration.Value.AcceptUnsignedImport)
            {
                ModelState.AddModelError(nameof(Signature), "Signature is required because 'AcceptUnsignedImport' is set to false.");
                ModelState.AddModelError(nameof(SignatureFile), "Signature is required because 'AcceptUnsignedImport' is set to false.");
                return Page();
            }

            if (Signature is not null)
            {
                var cert = new X509Certificate2(postsConfiguration.Value.CertificateFile);
                if (!Verify(JsonDocument!, Signature, cert))
                {
                    ModelState.AddModelError(nameof(Signature), "Signature is invalid.");
                    ModelState.AddModelError(nameof(SignatureFile), "Signature is invalid.");
                    return Page();
                }
            }

            switch (Entity.ToLower())
            {
                case "posts":
                    {
                        var entities = JsonSerializer.Deserialize<IEnumerable<Post>>(JsonDocument!) ?? [];
                        foreach (var entity in entities)
                        {
                            var post = context.Posts.SingleOrDefault(x => x.UniqueId == entity.UniqueId);
                            if (post is null)
                            {
                                entity.Id = 0;
                                context.Posts.Add(entity);
                            }
                            else
                            {
                                post.MdContent = entity.MdContent;
                                post.Title = entity.Title;
                                post.UpdatedAt = DateTime.Now;
                                post.IsPublished = entity.IsPublished;
                                post.MdPreview = entity.MdPreview;
                                post.CreatedAt = entity.CreatedAt;
                            }
                        }

                        await context.SaveChangesAsync();
                        return RedirectToPage("./Index");
                    }
                case "postimages":
                    {
                        var entities = JsonSerializer.Deserialize<IEnumerable<PostImage>>(JsonDocument!) ?? [];
                        foreach (var entity in entities)
                        {
                            var postImage = context.PostImages.SingleOrDefault(x => x.UniqueId == entity.UniqueId);
                            if (postImage is null)
                            {
                                entity.Id = 0;
                                context.PostImages.Add(entity);
                            }
                            else
                            {
                                postImage.AltText = entity.AltText;
                                postImage.ContentType = entity.ContentType;
                                postImage.Name = entity.Name;
                                postImage.Image = entity.Image;
                                postImage.Filename = entity.Filename;
                                postImage.UniqueId = entity.UniqueId;
                                postImage.UpdatedAt = DateTime.Now;
                                postImage.IsPublished = entity.IsPublished;
                                postImage.UpdatedAt = entity.UpdatedAt;
                                postImage.CreatedAt = entity.CreatedAt;
                            }
                        }

                        await context.SaveChangesAsync();
                        return RedirectToPage("./Index");
                    }
                default:
                    return RedirectToPage("./Index");
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
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
