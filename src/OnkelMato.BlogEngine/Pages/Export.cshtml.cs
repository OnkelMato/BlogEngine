using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Repository.Model;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OnkelMato.BlogEngine.Pages
{
    public class ExportModel(
        BlogEngineImportExportRepository importExportRepository,
        BlogEngineReadRepository readRepository,
        IOptions<ImportExportConfiguration> importExportConfiguration
    ) : PageModel
    {
        public bool AllowExportJson => importExportConfiguration.Value.EnableJsonExport;
        public bool AllowExportJwt => importExportConfiguration.Value.EnableJwtExport;
        public int SelectedCertificate { get; set; }
        public string[] Certificates => importExportConfiguration.Value.JwtPrivateCertificates;

        [Display(Name = "Certificate Password")]
        public string? CertificatePassword { get; set; }

        public string? SignaturePrivateKeys { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FormType { get; set; } // Allowed values: "json", "jwt", "sync"

        [BindProperty(SupportsGet = true)]
        public string? ExportType { get; set; } = "blog";// Allowed values: "blog", "post", "postImage"

        [BindProperty(SupportsGet = true)]
        public Guid? Id { get; set; } // Id of the entity in case it is post or postImage


        public async Task<IActionResult> OnGetAsync()
        {
            SignaturePrivateKeys = string.Join(',', importExportConfiguration.Value.JwtPrivateCertificates);

            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var blogOrParts = await GetBlogOrRequestedParts();
            if (blogOrParts is null) return BadRequest();

            if (string.Compare(FormType, "jwt", StringComparison.InvariantCultureIgnoreCase) == 0)
                return await ExportJwt(blogOrParts);


            return BadRequest();
        }

        private async Task<BlogExportModel?> GetBlogOrRequestedParts()
        {
            if (ExportType is null) return null;

            var blog = importExportRepository.Blog;

            var exportedEntities = new BlogExportModel();
            var filename = string.Empty;

            switch (ExportType.ToLower())
            {
                // todo this could be a strategy pattern
                case "blog":
                    {
                        // load full blog
                        blog = await readRepository.GetEntireBlog();

                        exportedEntities = CreateBlogExport(blog);
                        // todo change me to sth else. a method?
                        filename = $"{blog!.Title}.{DateTime.Now:yyyyMMdd-hhmm}";

                        break;
                    }
                //case "posts":
                //    {
                //        exportedEntities = CreatePostExport(Id, blog);
                //        if (exportedEntities.Posts.Count == 0)
                //            throw new ArgumentException($"No posts found for {Id}");

                //        filename = Id == null
                //            ? $"{blog!.Title}.GetEntireBlog.{DateTime.Now:yyyyMMdd-hhmm}"
                //            : $"{exportedEntities.Posts.First().Title}.{DateTime.Now:yyyyMMdd-hhmm}";

                //        break;
                //    }
                //case "postimages":
                //    {
                //        exportedEntities = CreatePostImageExport(Id, blog);
                //        if (exportedEntities.PostImages.Count == 0)
                //            throw new ArgumentException($"No posts found for {Id}");

                //        filename = Id == null
                //            ? $"{blog!.Title}.AllPostImages.{DateTime.Now:yyyyMMdd-hhmm}"
                //            : $"{exportedEntities.PostImages.First().Name}.{DateTime.Now:yyyyMMdd-hhmm}";

                //        break;
                //    }
                default:
                    return null;
            }

            return exportedEntities;
        }

        #region jwt export

        private async Task<IActionResult> ExportJwt(BlogExportModel blogOrParts)
        {
            var exportAsJson = blogOrParts.AsJson();
            var cert = importExportConfiguration.Value.JwtPrivateCertificates[SelectedCertificate];

            // todo add certificate info in file??
            var filename = $"{blogOrParts.Title ?? "blog-export"}.{DateTime.Now:yyyyMMdd-hhmm}";

            filename = filename + ".jwt.json";
            var token = GetTokenForJson(exportAsJson);
            return File(Encoding.UTF8.GetBytes(token), "application/json", filename);
        }

        private string GetTokenForJson(object payload)
        {
            // todo the certificate is not validated
            X509Certificate2 cert = string.IsNullOrWhiteSpace(CertificatePassword)
                ? X509Certificate2.CreateFromPemFile(
                    importExportConfiguration.Value.JwtPublicCertificates[SelectedCertificate],
                    importExportConfiguration.Value.JwtPrivateCertificates[SelectedCertificate])
                : X509Certificate2.CreateFromEncryptedPemFile(
                    importExportConfiguration.Value.JwtPublicCertificates[SelectedCertificate],
                    CertificatePassword,
                    importExportConfiguration.Value.JwtPrivateCertificates[SelectedCertificate]);

            IJwtAlgorithm algorithm = new RS256Algorithm(cert);
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            const string key = null; // not needed if algorithm is asymmetric

            return encoder.Encode(payload, key);
        }

        #endregion

        #region Create Helper

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

        #endregion
    }
}
