using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Repository.Model;
using HashAlgorithmName = System.Security.Cryptography.HashAlgorithmName;

namespace OnkelMato.BlogEngine.Pages;

public class ImportModel(
    BlogEngineMgmtRepository mgmtRepository,
    BlogEngineImportExportRepository importExportRepository,
    IOptionsMonitor<ImportExportConfiguration> imexConfiguration) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public string? FormType { get; set; }

    public bool ValidateCertificates => imexConfiguration.CurrentValue.ValidateCertificates;

    #region Json Text and File Import

    public bool UseJsonTextInput => imexConfiguration.CurrentValue.EnableJsonStringImport;

    // Textarea supports direct json input
    [BindProperty]
    [Display(Name = "Json with Data")]
    public string? JsonDocument { get; set; }

    [BindProperty]
    [Display(Name = "Signature for Json")]
    public string? Signature { get; set; }

    public bool UseJsonFileInput = imexConfiguration.CurrentValue.EnableJsonFileImport;
    private string? _signaturePublicKeys;

    // File upload supports json file input
    [BindProperty]
    [Display(Name = "Json with Data")]
    public IFormFile? JsonDocumentFile { get; set; } = null!;

    [BindProperty]
    [Display(Name = "Signature for Json")]
    public IFormFile? SignatureFile { get; set; } = null!;

    #endregion

    #region  Sync from remote blog

    public bool UseSyncInput => imexConfiguration.CurrentValue.EnableBlogSync;

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Remote Blog Url")]
    public string RemoteSyncUrl { get; set; } = "https://";

    [BindProperty]
    [Display(Name = "Clear Blog before Sync")]
    public bool ClearBlogBeforeSync { get; set; } = true;

    #endregion

    #region JwtImport

    public bool UseJwtInput => imexConfiguration.CurrentValue.EnableJwtImport;

    // File upload supports json file input
    [BindProperty]
    [Display(Name = "JWT Export")]
    public IFormFile? JwtDocumentFile { get; set; } = null!;

    public string? SignaturePublicKeys => string.Join(",", imexConfiguration.CurrentValue.JwtPublicCertificates);

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Import as new Blog")]
    public bool ImportAsNewBlog { get; set; }

    #endregion

    public async Task<IActionResult> OnGetAsync()
    {
        ModelState.Clear();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // todo: change this to just load the blog document (json)
        // implement sth line a strategy pattern here. Single class strategy? Or just methods?
        // after json was loaded the procedure is the same (maybe except the clear blog)

        var importModelJson = string.Empty;
        var clearBlog = false;

        switch (FormType)
        {
            // this is the typical pattern for a strategy pattern. Could be refactored to strategy classes later.
            case "sync":
                importModelJson = await ImportJsonFromUrl(RemoteSyncUrl);
                clearBlog = ClearBlogBeforeSync;
                break;
            case "jwtFile":
                {
                    using var stream = new MemoryStream();
                    await JwtDocumentFile!.CopyToAsync(stream);
                    stream.Position = 0;
                    var jwt = await new StreamReader(stream).ReadToEndAsync();

                    importModelJson = await ImportJsonFromJwt(jwt);
                    break;
                }
            // this is why I don't link to many language features. It makes reading complex when you are new to the code (and the syntactic sugar)
            case "jsonText" when ValidateCertificates && !SignatureIsValid(JsonDocument, Signature):
                ModelState.AddModelError(nameof(Signature), "Signature is invalid.");
                return Page();
            case "jsonText":
                importModelJson = JsonDocument;
                break;
            case "jsonFile":
                {
                    using var stream = new MemoryStream();
                    await JsonDocumentFile!.CopyToAsync(stream);
                    stream.Position = 0;
                    var jsonDocumentTemp = await new StreamReader(stream).ReadToEndAsync();

                    if (ValidateCertificates)
                    {
                        using var stream2 = new MemoryStream();
                        await SignatureFile!.CopyToAsync(stream2);
                        stream2.Position = 0;
                        var signatureTemp = await new StreamReader(stream2).ReadToEndAsync();

                        if (!SignatureIsValid(jsonDocumentTemp, signatureTemp))
                        {
                            ModelState.AddModelError(nameof(Signature), "Signature is invalid.");
                            return Page();
                        }
                    }

                    importModelJson = jsonDocumentTemp;
                    break;
                }
            default:
                throw new Exception($"Form Type {FormType} not defined");
        }

        // clear blog is explicitly selected
        if (clearBlog)
            await importExportRepository.ClearBlog();

        // deserialize and import
        var importModel = JsonSerializer.Deserialize<BlogExportModel>(importModelJson) ?? throw new ArgumentNullException(nameof(importModelJson));

        if (ImportAsNewBlog)
        {
            var blogImportId = await mgmtRepository.DoImportAsNewBlog(importModel);
            return Redirect("/?blogId=" + blogImportId);
        }
        else
        {
            await importExportRepository.DoImportBlog(importModel);
            return Redirect("/");
        }

    }

    private async Task<string?> ImportJsonFromJwt(string token)
    {
        var result = string.Empty;

        foreach (var certificateFile in imexConfiguration.CurrentValue.JwtPublicCertificates)
        {
            try
            {
                var cert = new X509Certificate2(certificateFile);
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new RS256Algorithm(cert);
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var json = decoder.Decode(token, false);
                result = JsonSerializer.Deserialize<string>(json!);

                return result;
            }
            catch (TokenNotYetValidException)
            {
                // todo return to error page
            }
            catch (TokenExpiredException)
            {
                // todo return to error page
            }
            catch (SignatureVerificationException)
            {
                // todo return to error page
            }
        }

        return null;
    }


    /// <summary>
    /// Imports the JSON from a remote URL.
    /// </summary>
    /// <param name="remoteSyncUrl">The remote URL to import JSON from.</param>
    /// <returns>The imported JSON as a string.</returns>
    private async Task<string> ImportJsonFromUrl(string remoteSyncUrl)
    {
        try
        {
            // in case of certificate validation disabled
            var handler = new HttpClientHandler();
            
            // todo fixme
            // if (imexConfiguration.CurrentValue.ValidateCertificates)
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var client = new HttpClient(handler);
            using var response = await client.GetAsync(RemoteSyncUrl + "/Export?type=json");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (Exception e)
        {
            throw;
        }
    }


    private bool SignatureIsValid(string? jsonDocument, string? signature)
    {
        var result = false;
        foreach (var certificate in imexConfiguration.CurrentValue.JwtPublicCertificates)
        {
            var cert = new X509Certificate2(certificate);
            if (Verify(JsonDocument!, Signature, cert))
                result = true;
        }

        return result;
    }

    private static bool Verify(string data, string signature, X509Certificate2 serverCert)
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