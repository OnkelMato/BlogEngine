using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // set data protection key persistence
        try
        {
            var dataProtectionKeysPath = builder.Configuration.GetValue<string>("SystemConfig:DataProtectionKeysPath") ?? string.Empty;
            Directory.CreateDirectory(dataProtectionKeysPath);
            if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath) && Directory.Exists(dataProtectionKeysPath))
                builder.Services.AddDataProtection()
                    .UseCryptographicAlgorithms(
                        new AuthenticatedEncryptorConfiguration
                        {
                            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                        })
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        }
        catch (Exception ex)
        {
            Trace.TraceError("Cannot set data protection key persistence. Details: " + ex.Message);
        }

        // add all blog engine infrastructure services
        builder.AddBlogEngine();
        builder.Services.AddScoped<ILinkFactory, BlogEngineLinkFactory>();
        var enableRssFeed = builder.Configuration.GetValue<bool>("Blog:EnableRssFeed");
        if (enableRssFeed) builder.AddRssFeed();

        // add routing for blog posts to be more SEO friendly
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Posts", "/Post/{titleStub}/{id}");
        });
        builder.Services.AddBlogSeoTags();


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
