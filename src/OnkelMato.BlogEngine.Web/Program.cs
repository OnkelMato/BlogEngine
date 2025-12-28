using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using System;
using System.Diagnostics;
using System.IO;

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

// static class for seo extensions
public static class SeoExtensions
{
    public static IServiceCollection AddBlogSeoTags(this IServiceCollection services)
    {
        //return services;
        // todo this must be done later. blog id is unknown here. 
       
        //Register your services
        services.AddSeoTags(seoInfo =>
        {
            var serviceProvider = services.BuildServiceProvider();
            //var db = serviceProvider.GetService<BlogEngineReadRepository>() ?? throw new Exception("Cannot init Database");
            var blogSettings = serviceProvider.GetService<IOptionsMonitor<BlogConfiguration>>() ?? throw new Exception("Cannot get blog settings");
            //var blogTitle = db.Blog()?.Title ?? "Onkel Mato Blog Engine";
            var blogTitle = "Onkel Mato Blog Engine";

            seoInfo.SetSiteInfo(
                siteTitle: blogTitle,
                //openSearchUrl: "https://site.com/open-search.xml",  //Optional
                robots: "index, follow"                             //Optional
            );

            //Optional
            //seoInfo.AddFeed(
            //    title: "Post Feeds",
            //    url: "https://site.com/rss/",
            //    feedType: FeedType.Rss);

            //Optional
            seoInfo.SetLocales(blogSettings.CurrentValue.Language);
        });
        return services;
    }
}