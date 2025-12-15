using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository.Model;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Core.Repository;

public class BlogEngineMgmtRepository
{
    private readonly BlogEngineContext _context;

    public BlogEngineMgmtRepository(
        BlogEngineContext context,
        IBlogIdProvider blogId)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task ClearPostsAndImages(Guid blogId)
    {
        foreach (var e in _context.PostImages.Where(x => x.Blog.UniqueId == blogId))
            _context.PostImages.Remove(e);
        foreach (var e in _context.Posts.Where(x => x.Blog.UniqueId == blogId))
            _context.Posts.Remove(e);
    }

    public async Task DeleteBlog(Guid blogId)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(x => x.UniqueId == blogId);
        if (blog == null) return;

        _context.Blogs.Remove(blog!);
        await _context.SaveChangesAsync();
    }

    public IEnumerable<Blog> GetAllBlogs()
    {
        return _context.Blogs.Select(x => x.ToModel()).ToArray();
    }

    public async Task<Guid> DoImportAsNewBlog(BlogExportModel blogExport)
    {
        var newBlog = await CreateBlog(Guid.NewGuid());

        var repo = new BlogEngineImportExportRepository(_context, new StaticBlogIdProvider(newBlog));
        await repo.DoImportBlog(blogExport);

        return newBlog;
    }

    public async Task<Guid> CreateBlog(Guid id)
    {
        if (id == Guid.Empty) return id;

        // create blog if not exists and use it
        var blog = new BlogDb()
        {
            UniqueId = id,
            Title = $"My Blog #{id}",
            Description = "My Blog",
            CSS = string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        _context.Blogs.Add(blog);

        _context.Posts.AddFooterLink(blog, Guid.Parse("F5C50612-DAAA-44A3-AA2B-49090DE41F90"), "Onkel Mato Blog Engine", "https://github.com/OnkelMato/BlogEngine");
        _context.Posts.AddFooterLink(blog, Guid.Parse("1A12B373-7E48-4E01-B040-DA896CE67F75"), "Admin", "/Admin", 20000);
        _context.Posts.AddFooter(blog, Guid.Parse("1B5FBEA4-10FE-46DD-AC59-7573F54DFDBB"), "Legal Notice", "## Legal Notice", 15100);
        _context.Posts.AddFooter(blog, Guid.Parse("D20F7182-1CB6-4A22-AD20-3B49B47D391F"), "Data Protection", "## Data protection", 15000);

        await _context.SaveChangesAsync();

        return id;
    }

    [Obsolete]
    public IEnumerable<Blog> GetBlogs()
    {
        return _context.Blogs.Select(x => x.ToModel()).ToArray();
    }

    public bool HasBlog(Guid blogId)
    {
        return _context.Blogs.Any(x => x.UniqueId == blogId);
    }

    public async Task<Blog?> GetFirstBlogOrDefault()
    {
        return (await _context.Blogs.FirstOrDefaultAsync())?.ToModel();
    }
}