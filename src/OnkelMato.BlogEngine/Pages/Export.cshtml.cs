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
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

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
        public int SelectedCertificate { get; set; } = -1;
        public string[] Certificates => importExportConfiguration.Value.JwtPrivateCertificates;

        [Display(Name = "Certificate Password")]
        [BindProperty]
        public string? CertificatePassword { get; set; }

        [Display(Name = "Certificate Password")]
        [BindProperty]
        public string? CertificatePasswordRemote { get; set; }

        public string? SignaturePrivateKeys { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FormType { get; set; } // Allowed values: "json", "jwt", "sync"

        [BindProperty(SupportsGet = true)]
        public string? ExportType { get; set; } = "blog";// Allowed values: "blog", "post", "postimage"

        [BindProperty(SupportsGet = true)]
        public Guid? Id { get; set; } // Id of the entity in case it is post or postImage

        [BindProperty(SupportsGet = true)]
        public string SelectedJsonFormat { get; set; } = "JSON";

        public string? Scope { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RemoteSyncUrl { get; set; }

        public bool AllowExportRemote => importExportConfiguration.Value.EnableRemoteExport;

        [BindProperty(SupportsGet = true)]
        public int SelectedCertificateRemote { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            SignaturePrivateKeys = string.Join(',', importExportConfiguration.Value.JwtPrivateCertificates);
            if (string.Compare(ExportType, "blog", StringComparison.InvariantCultureIgnoreCase) == 0) Scope = "Entire Blog";
            if (string.Compare(ExportType, "post", StringComparison.InvariantCultureIgnoreCase) == 0)
                Scope = Id == null ? "All Posts" : $"Post: {(await readRepository.PostAsync(Id.Value))!.Title}";
            if (string.Compare(ExportType, "postimage", StringComparison.InvariantCultureIgnoreCase) == 0)
                Scope = Id == null ? "All Post Images" : $"Post Image: {(await readRepository.PostImageAsync(Id.Value))!.Name}";

            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var blogOrParts = await GetBlogOrRequestedParts();
            if (blogOrParts is null) return BadRequest();

            if (string.Compare(FormType, "jwt", StringComparison.InvariantCultureIgnoreCase) == 0 && AllowExportJwt)
                return await ExportJwt(blogOrParts);

            if (string.Compare(FormType, "json", StringComparison.InvariantCultureIgnoreCase) == 0 && AllowExportJson)
                return await ExportJson(blogOrParts);

            if (string.Compare(FormType, "sync", StringComparison.InvariantCultureIgnoreCase) == 0 && AllowExportRemote)
                return await ExportRemote(blogOrParts);

            return BadRequest();
        }

        private async Task<BlogExportModel?> GetBlogOrRequestedParts()
        {
            if (ExportType is null) return null;

            switch (ExportType.ToLower())
            {
                // todo this could be a strategy pattern
                case "blog":
                    {
                        var blog = await readRepository.GetEntireBlog();
                        return blog is null ? null : CreateBlogExport(blog);
                    }
                case "post":
                    {
                        if (Id is null) return null;
                        var post = await readRepository.PostAsync(Id.Value);
                        return post is null ? null : CreateExport(post);
                    }
                case "postimage":
                    {
                        if (Id is null) return null;
                        var postImage = await readRepository.PostImageAsync(Id.Value);
                        return postImage is null ? null : CreateExport(postImage);
                    }
                default:
                    return null;
            }
        }

        #region remote export

        private async Task<IActionResult> ExportRemote(BlogExportModel blogOrParts)
        {
            if (SelectedCertificateRemote < 0 || SelectedCertificateRemote >= importExportConfiguration.Value.JwtPrivateCertificates.Length)
            {
                ModelState.AddModelError(nameof(SelectedCertificateRemote), "no certificate selected");
                return Page();
            }

            if (RemoteSyncUrl is null)
            {
                ModelState.AddModelError(nameof(RemoteSyncUrl), "no remote sync url provided");
                return Page();
            }

            var exportAsJson = blogOrParts.AsJson();

            // get token for upload
            var tokenResult = GetTokenForJson(exportAsJson, SelectedCertificateRemote, CertificatePasswordRemote, out var issuer);
            if (tokenResult.IsFailure)
            {
                ModelState.AddModelError(nameof(CertificatePasswordRemote), tokenResult.ErrorMessage!);
                return Page();
            }
            var token = tokenResult.Value!;
            var result = await UploadToRemoteViaHttp(token, RemoteSyncUrl, null);
            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                return Page();
            }

            return Page();
            return Redirect(RemoteSyncUrl!);
        }

        private async Task<ModelResult<object>> UploadToRemoteViaHttp(string token, string remoteSyncUrl, Guid? blogId)
        {
            // create http client that accepts any server certificate (for self-signed certificates) in case it is configured
            var handler = new HttpClientHandler();
            if (!importExportConfiguration.Value.ValidateCertificates)
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var client = new HttpClient(handler);

            // load the import dialog to get an antiforgery token from server
            var aftResponse = await client.GetAsync(RemoteSyncUrl!.TrimEnd('/') + "/Import");
            var doc = await aftResponse.Content.ReadAsStringAsync();
            var aft = ExtractAntiforgeryTokenFromHtml(doc, string.Empty);
            if (aft is null) return ModelResult<object>.Failure("Failed to extract antiforgery token from remote server.");

            // add token, file and form type to multipart form data
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("jwtFile"), "FormType");
            form.Add(new StringContent(aft), "__RequestVerificationToken");

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(token));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            form.Add(content, "JwtDocumentFile", "export.json");

            // post the data
            var response = await client.PostAsync(RemoteSyncUrl!.TrimEnd('/') + "/Import", form);

            return response.IsSuccessStatusCode
                ? ModelResult<object>.Success(new object())
                : ModelResult<object>.Failure($"Failed to upload data: {response.StatusCode}");
        }

        private string? ExtractAntiforgeryTokenFromHtml(string html, string divIdOrName)
        {
            // todo fix when divIdOrName is empty string or not found.
            try
            {
                var searchContent = html;
                // find the div with the given name or id. If not found search take all content.
                var divPattern = $@"<div[^>]*(?:id|name)=""{divIdOrName}""[^>]*>(.*?)</div>";
                var divMatch = Regex.Match(html, divPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (divMatch.Success)
                    searchContent = divMatch.Groups[1].Value;

                // search for hidden field __RequestVerificationToken within the searchContent
                const string tokenPattern = @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]*)""[^>]*/?>";
                var tokenMatch = Regex.Match(searchContent, tokenPattern, RegexOptions.IgnoreCase);

                return tokenMatch.Success ? tokenMatch.Groups[1].Value : null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region json export

        private async Task<IActionResult> ExportJson(BlogExportModel blogOrParts)
        {
            var asPretty = string.Compare("jsonpretty", SelectedJsonFormat, StringComparison.InvariantCultureIgnoreCase) == 0;
            var exportAsJson = blogOrParts.AsJson(asPretty);

            var filename = GenerateFilename(blogOrParts);

            filename = $"{filename}.json";
            return File(Encoding.UTF8.GetBytes(exportAsJson), "application/json", filename);
        }
        #endregion

        #region jwt export

        private async Task<IActionResult> ExportJwt(BlogExportModel blogOrParts)
        {
            if (SelectedCertificate < 0 || SelectedCertificate >= importExportConfiguration.Value.JwtPrivateCertificates.Length)
            {
                ModelState.AddModelError(nameof(SelectedCertificate), "no certificate selected");
                return Page();
            }

            var exportAsJson = blogOrParts.AsJson();

            var tokenResult = GetTokenForJson(exportAsJson, SelectedCertificate, CertificatePassword, out var issuer);
            if (tokenResult.IsFailure)
            {
                ModelState.AddModelError(nameof(CertificatePassword), tokenResult.ErrorMessage!);
                return Page();
            }

            var filename = GenerateFilename(blogOrParts);
            filename = $"{filename}.{issuer}.jwt";
            return File(Encoding.UTF8.GetBytes(tokenResult.Value!), "application/json", filename);
        }

        private ModelResult<string> GetTokenForJson(object payload, int selectedCertificate, string? certificatePassword, out string issuer)
        {
            // open certificate and get issuer
            X509Certificate2 cert;
            try
            {
                cert = string.IsNullOrWhiteSpace(certificatePassword)
                    ? X509Certificate2.CreateFromPemFile(
                        importExportConfiguration.Value.JwtPublicCertificates[selectedCertificate],
                        importExportConfiguration.Value.JwtPrivateCertificates[selectedCertificate])
                    : X509Certificate2.CreateFromEncryptedPemFile(
                        importExportConfiguration.Value.JwtPublicCertificates[selectedCertificate],
                        certificatePassword,
                        importExportConfiguration.Value.JwtPrivateCertificates[selectedCertificate]);
                issuer = cert.Issuer.Split(',')[0].Split('=')[1];
            }
            catch (Exception e)
            {
                issuer = string.Empty;
                return ModelResult<string>.Failure(e.Message);
            }

            // validate certificate
            if (importExportConfiguration.Value.ValidateCertificates && !cert.Verify())
                return ModelResult<string>.Failure("certificate is not valid");

            // create the jwt token
            IJwtAlgorithm algorithm = new RS256Algorithm(cert);
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return ModelResult<string>.Success(encoder.Encode(payload, (string)null!));
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

        private BlogExportModel CreateExport(Post post)
        {
            // todo find all images im post!
            var export = new BlogExportModel()
            {
                Posts = [post.ToPostExportModel()],
                IsFullExport = false
            };

            return export;
        }

        private BlogExportModel CreateExport(PostImage postImage)
        {
            var export = new BlogExportModel()
            {
                PostImages = [postImage.ToPostImageExportModel()],
                IsFullExport = false
            };

            return export;
        }

        #endregion

        #region filename helper

        private string GenerateFilename(BlogExportModel blog)
        {
            if (!string.IsNullOrWhiteSpace(blog.Title))
                return $"{blog.Title}.{DateTime.Now:yyyyMMdd-hhmm}";

            if (!string.IsNullOrWhiteSpace(blog.Posts.FirstOrDefault()?.Title))
                return $"{blog.Posts.FirstOrDefault()!.Title}.{DateTime.Now:yyyyMMdd-hhmm}";

            if (!string.IsNullOrWhiteSpace(blog.PostImages.FirstOrDefault()?.Name))
                return $"{blog.PostImages.FirstOrDefault()!.Name}.{DateTime.Now:yyyyMMdd-hhmm}";

            return $"Blog-Export.{DateTime.Now:yyyyMMdd-hhmm}";
        }


        #endregion
    }
}
