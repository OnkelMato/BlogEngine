using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository.Model;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Core.Repository;

public class BlogEngineImportExportRepository
{
    private readonly BlogEngineContext _context;
    private readonly IBlogIdProvider _blogId;
    private readonly Lazy<BlogDb?> _lazyBlog;

    public BlogEngineImportExportRepository(
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

    // todo caching here
    public Blog Blog => _lazyBlog.Value!.ToModel();

    public async Task DoImportBlog(BlogExportModel blogExport)
    {
        var blog = _lazyBlog.Value!;
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

    private void DoImportPost(BlogExportModel.PostExportModel postExport)
    {
        var blog = _lazyBlog.Value!;
        var postEntity = _context.Posts.SingleOrDefault(x => x.UniqueId == postExport.UniqueId && x.Blog == blog);
        var headerImage = _context.PostImages.Local.SingleOrDefault(x => x.UniqueId == postExport.HeaderImage && x.Blog == blog)
                          ?? _context.PostImages.SingleOrDefault(x => x.UniqueId == postExport.HeaderImage && x.Blog == blog);
        if (postEntity is null)
        {
            postEntity = new PostDb()
            {
                UniqueId = postExport.UniqueId,
                Blog = blog,
                HeaderImage = headerImage,
                MdContent = postExport.MdContent,
                Title = postExport.Title,
                UpdatedAt = DateTime.Now,
                ShowState = (ShowStateDb)postExport.ShowState,
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
            postEntity.ShowState = (ShowStateDb)postExport.ShowState;
            postEntity.MdPreview = postExport.MdPreview;
            postEntity.CreatedAt = postExport.CreatedAt;
            postEntity.PublishedAt = (postExport.PublishedAt == DateTime.MinValue) ? postExport.CreatedAt : postExport.PublishedAt;
            _context.Posts.Update(postEntity);
        }
    }

    private void DoImportPostImage(BlogExportModel.PostImageExportModel postImageExport)
    {
        var blog = _lazyBlog.Value!;

        var postImageEntity = _context.PostImages.SingleOrDefault(x => x.UniqueId == postImageExport.UniqueId && x.Blog == blog);
        if (postImageEntity is null)
        {
            postImageEntity = new PostImageDb()
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

    public async Task ClearBlog()
    {
        foreach (var e in _context.PostImages.Where(x => x.Blog == _lazyBlog.Value))
            _context.PostImages.Remove(e);

        foreach (var e in _context.PostTags.Where(x => x.Blog == _lazyBlog.Value))
            _context.PostTags.Remove(e);

        foreach (var e in _context.Posts.Where(x => x.Blog == _lazyBlog.Value))
            _context.Posts.Remove(e);
    }

}