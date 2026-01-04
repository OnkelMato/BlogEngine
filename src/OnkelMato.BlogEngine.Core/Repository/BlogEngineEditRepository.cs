using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Service;
using System.Data.Common;

namespace OnkelMato.BlogEngine.Core.Repository;

public class BlogEngineEditRepository
{
    private readonly BlogEngineContext _context;
    private readonly IBlogIdProvider _blogId;
    private readonly Lazy<BlogDb?> _lazyBlog;

    public BlogEngineEditRepository(
        BlogEngineContext context,
        IBlogIdProvider blogId)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _blogId = blogId;

        _lazyBlog = new Lazy<BlogDb?>(() =>
        {
            var blog = _context.Blogs.FirstOrDefault(x => x.UniqueId == blogId.Id);

            return blog;
        });
    }

    public Blog Blog => _lazyBlog.Value?.ToModel() ?? throw new InvalidOperationException("Blog not found");

    public async Task<bool> UpdateBlog(string title, string? description, string? css)
    {
        var blogUid = _blogId.Id;
        var dbBlog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == blogUid);
        if (dbBlog == null) return false;

        dbBlog.Title = title;
        dbBlog.Description = description;
        dbBlog.UpdatedAt = DateTime.Now;
        dbBlog.CSS = css;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbException)
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

    public async Task Delete(Post post)
    {
        var entity = await _context.Posts.SingleAsync(x => x.Blog == _lazyBlog.Value && x.UniqueId == post.UniqueId);
        _context.Posts.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(PostImage postImage)
    {
        var entity = await _context.PostImages.SingleAsync(x => x.Blog == _lazyBlog.Value && x.UniqueId == postImage.UniqueId);
        _context.PostImages.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<RepositoryResult<PostImage>> UpdateImage(PostImage postImage)
    {
        var entity = await _context.PostImages.SingleOrDefaultAsync(x => x.UniqueId == postImage.UniqueId && x.Blog == _lazyBlog.Value);
        if (entity == null) return RepositoryResult<PostImage>.Failure($"Cannot find image with id {postImage.UniqueId}");

        entity.AltText = postImage.AltText;
        entity.IsPublished = postImage.IsPublished;
        entity.Name = postImage.Name;
        entity.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbException e)
        {
            return RepositoryResult<PostImage>.Failure($"Cannot save image with id {postImage.UniqueId}: {e.Message}");
        }

        return RepositoryResult<PostImage>.Success(postImage);
    }

    public async Task<RepositoryResult<PostImage>> AddImage(PostImage model)
    {
        var entity = new PostImageDb()
        {
            Blog = _lazyBlog.Value ?? throw new InvalidOperationException("Blog not found"),
            UniqueId = Guid.NewGuid(),
            Image = model.Image,
            Name = model.Name,
            ContentType = model.ContentType,
            AltText = model.AltText,
            IsPublished = model.IsPublished,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.PostImages.Add(entity);
        await _context.SaveChangesAsync();

        model.UniqueId = entity.UniqueId;
        model.CreatedAt = entity.CreatedAt;
        model.UpdatedAt = entity.UpdatedAt;
        return RepositoryResult<PostImage>.Success(model);
    }

    public async Task<RepositoryResult<Post>> UpdatePost(Post post)
    {
        var dbPost = await _context.Posts
            .Include(x=> x.HeaderImage)
            .Include(x => x.PostTags)
            .SingleAsync(m => m.UniqueId == post.UniqueId && m.Blog == _lazyBlog.Value);

        var dbImage = post.HeaderImage != null
            ? await _context.PostImages.SingleOrDefaultAsync(m => m.UniqueId == post.HeaderImage.UniqueId && m.Blog == _lazyBlog.Value)
            : null;
        // make more validations! dbImage == null is a problem

        dbPost.MdContent = post.MdContent;
        dbPost.Title = post.Title;
        dbPost.Order = post.Order;
        dbPost.PublishedAt = post.PublishedAt;
        dbPost.UpdatedAt = DateTime.Now;
        dbPost.HeaderImage = dbImage;
        dbPost.ShowState = post.ShowState.FromModel();
        dbPost.MdPreview = post.MdPreview;

        // todo not the best solution but works

        // check dbPost.PostTags with post.Tags and add new and remove missing
        dbPost.PostTags.Clear();
        dbPost.PostTags.AddRange(post.PostTags.Distinct().Select(x => new PostTagDb()
        {
            Blog = dbPost.Blog,
            Post = dbPost,
            Title = x
        }));

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbException e)
        {
            return RepositoryResult<Post>.Failure($"Cannot save post with id {post.UniqueId}: {e.Message}");
        }

        return RepositoryResult<Post>.Success(post);
    }

    public async Task<RepositoryResult<Post>> AddPost(Post post)
    {
        var dbImage = post.HeaderImage != null
            ? await _context.PostImages.SingleAsync(m => m.UniqueId == post.HeaderImage.UniqueId && m.Blog == _lazyBlog.Value)
            : null;

        var dbPost = _context.Posts.Add(new PostDb()
        {
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UniqueId = Guid.NewGuid(),
            Blog = _lazyBlog.Value!,
            HeaderImage = dbImage,
            MdContent = post.MdContent,
            PublishedAt = post.PublishedAt,
            Title = post.Title,
            Order = post.Order,
            ShowState = post.ShowState.FromModel(),
            MdPreview = post.MdPreview,
        });
        dbPost.Entity.PostTags = post.PostTags
            .Distinct().Select(x => new PostTagDb() { Post = dbPost.Entity, Blog = _lazyBlog.Value!, Title = x }).ToList();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbException e)
        {
            return RepositoryResult<Post>.Failure($"Cannot save post with id {post.UniqueId}: {e.Message}");
        }

        return RepositoryResult<Post>.Success(post);
    }
}