using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database.Entity;

namespace OnkelMato.BlogEngine.Core.Repository;

public static class DbSetPostExtensions
{
    public static void AddFooter(this DbSet<PostDb> source, BlogDb blog, Guid id, string title, string content,
        int order = 10000)
    {
        var post = new PostDb()
        {
            Title = title,
            MdContent = content,
            MdPreview = ".",
            ShowState = ShowStateDb.Footer,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            UniqueId = id,
            Order = order,
            Blog = blog,
        };

        source.Add(post);
    }

    public static void AddFooterLink(this DbSet<PostDb> source, BlogDb blog, Guid id, string title, string link,
        int order = 10000)
    {
        var post = new PostDb()
        {
            Title = title,
            MdPreview = link,
            ShowState = ShowStateDb.LinkAndFooter,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            UniqueId = id,
            Order = order,
            Blog = blog,
        };

        source.Add(post);
    }
}