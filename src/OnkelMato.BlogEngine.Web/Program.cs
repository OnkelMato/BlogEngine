using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OnkelMato.BlogEngine.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //string dataProtectionKeysPath;
        //try
        //{
        //     dataProtectionKeysPath = builder.Configuration.GetValue<string>("SystemConfig:DataProtectionKeysPath") ?? string.Empty; ;
        //}
        //catch (Exception)
        //{
        //    dataProtectionKeysPath = string.Empty;
        //}

        builder.AddBlogEngine();
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Posts", "/Post/{titleStub}/{id}");
        });
        builder.Services.AddBlogSeoTags();

        //if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath) && Directory.Exists(dataProtectionKeysPath))
        //    builder.Services.AddDataProtection()
        //        .UseCryptographicAlgorithms(
        //            new AuthenticatedEncryptorConfiguration
        //            {
        //                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        //                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        //            })
        //        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            //app.UseMigrationsEndPoint();
        }
        else
        {
            //app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.EnsureDatabase();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}
