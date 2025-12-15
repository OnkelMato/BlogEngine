using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Database;
using OnkelMato.BlogEngine.Pages;
using System;
using static OnkelMato.BlogEngine.Pages.BlogExportModel;

namespace OnkelMato.BlogEngine;

public class BlogEngineRepository
{
    private readonly BlogEngineContext _context;
    //private readonly IOptionsMonitor<BlogConfiguration> _settings;
    private readonly IBlogIdProvider _blogId;
    private readonly Lazy<Blog?> _lazyBlog;

    public BlogEngineRepository(
        BlogEngineContext context,
        IBlogIdProvider blogId)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _blogId = blogId;

        _lazyBlog = new Lazy<Blog?>(() =>
        {
            var blog = _context.Blogs.FirstOrDefault(x => x.UniqueId == blogId.Id);

            return blog;
        });
    }

    /// <summary>
    /// return the blog without (!) loading the posts and images
    /// </summary>
    /// <exception cref="InvalidOperationException">Configured blog cannot be found</exception>
    public Blog? Blog() => _lazyBlog.Value;


    public int PostsOnBlogCount()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            .Count(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Blog || x.ShowState == ShowState.BlogAndMenu || x.ShowState == ShowState.BlogAndFooter));
    }

    public IEnumerable<Post> PostsOnBlog(int currentPage, int postsPerPage)
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        if (postsPerPage <= 0)
            throw new ArgumentOutOfRangeException(nameof(postsPerPage), "PostsPerPage must be greater than zero");

        return _context.Posts
            .Include(x => x.HeaderImage)
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Blog || x.ShowState == ShowState.BlogAndMenu || x.ShowState == ShowState.BlogAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt)
            .Skip((currentPage - 1) * postsPerPage)
            .Take(postsPerPage);
    }

    public Post? Post(Guid id)
    {
        return _context.Posts
            .Include(x => x.HeaderImage)
            .SingleOrDefault(x => x.Blog == _lazyBlog.Value && x.UniqueId == id);
    }

    public IEnumerable<Post> PostsInMenu()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            // todo use flag and or
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Menu || x.ShowState == ShowState.BlogAndMenu || x.ShowState == ShowState.LinkAndMenu))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt);
    }

    public IEnumerable<Post> PostsInFooter()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            // todo use flag and or
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Footer || x.ShowState == ShowState.BlogAndFooter || x.ShowState == ShowState.LinkAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt);
    }

    public PostImage? GetImage(Guid imageId)
    {
        return _context.PostImages.SingleOrDefault(x => x.UniqueId == imageId && x.Blog == _lazyBlog.Value);
    }

    public IEnumerable<Blog> GetBlogs()
    {
        return _context.Blogs.ToArray();
    }

    public async Task<bool> UpdateBlog(string title, string? description, string css)
    {
        var blogUid = _blogId.Id;
        var dbBlog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == blogUid);
        if (dbBlog == null) return false;

        dbBlog.Title = title;
        dbBlog.Description = description;
        dbBlog.UpdatedAt = DateTime.Now;
        // todo dbBlog.CSS = Blog.CSS;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Blogs.Any(e => e.UniqueId == blogUid))
            {
                return false;
            }
            else
            {
                throw;
            }
        }

        return true;
    }

    public async Task<Post?> GetPostsWithHeaderImages(Guid uniqueId)
    {
        return await _context
            .Posts
            .Include(x => x.HeaderImage)
            .SingleOrDefaultAsync(m => m.UniqueId == uniqueId && m.Blog == _lazyBlog.Value);
    }

    #region Import Blog

    public async Task DoImportBlog(BlogExportModel blogExport)
    {
        var blog = _lazyBlog.Value;
        var blogNameExists = _context.Blogs.Any(x => x.Title == blogExport.Title && x.UniqueId != blog.UniqueId);
        var blogAlternateName = $"{blogExport.Title} ({_blogId.Id})";

        // in case only a post or image was exported, the export values will be null.
        if (blogExport.IsFullExport)
        {
            blog.Title = blogNameExists ? blogAlternateName : (blogExport.Title ?? blog.Title);
            blog.Description = blogExport.Description;
            blog.UpdatedAt = DateTime.Now;
            blog.CreatedAt = blogExport.CreatedAt;

            _context.Blogs.Update(blog);
        }

        foreach (var postImage in blogExport.PostImages)
            DoImportPostImage(postImage);
        //context.SaveChanges();

        foreach (var post in blogExport.Posts)
            DoImportPost(post);

        await _context.SaveChangesAsync();
    }

    private void DoImportPost(PostExportModel postExport)
    {
        var blog = _lazyBlog.Value;
        var postEntity = _context.Posts.SingleOrDefault(x => x.UniqueId == postExport.UniqueId && x.Blog == blog);
        var headerImage = _context.PostImages.Local.SingleOrDefault(x => x.UniqueId == postExport.HeaderImage && x.Blog == blog)
                          ?? _context.PostImages.SingleOrDefault(x => x.UniqueId == postExport.HeaderImage && x.Blog == blog);
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
                CreatedAt = postExport.CreatedAt,
                PublishedAt = postExport.PublishedAt
            };
            _context.Posts.Add(postEntity);
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
            postEntity.PublishedAt = (postExport.PublishedAt == DateTime.MinValue) ? postExport.CreatedAt : postExport.PublishedAt;
            _context.Posts.Update(postEntity);
        }
    }

    private void DoImportPostImage(PostImageExportModel postImageExport)
    {
        var blog = _lazyBlog.Value;

        var postImageEntity = _context.PostImages.SingleOrDefault(x => x.UniqueId == postImageExport.UniqueId && x.Blog == blog);
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
            _context.PostImages.Add(postImageEntity);
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
            _context.PostImages.Update(postImageEntity);
        }
    }

    public async Task ClearPostsAndImages()
    {
        foreach (var e in _context.PostImages.Where(x => x.Blog == _lazyBlog.Value))
            _context.PostImages.Remove(e);
        foreach (var e in _context.Posts.Where(x => x.Blog == _lazyBlog.Value))
            _context.Posts.Remove(e);
    }



    #endregion
}