using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;
using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine;

public class BlogEngineRepository
{
    private readonly BlogEngineContext _context;
    private readonly IOptionsMonitor<PostsConfiguration> _settings;
    private readonly Lazy<Blog?> _lazyBlog;

    public BlogEngineRepository(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> settings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        _lazyBlog = new Lazy<Blog?>(() =>
        {
            var blog = _context.Blogs.FirstOrDefault(x => x.UniqueId == _settings.CurrentValue.BlogUniqueId);

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
            throw new InvalidOperationException($"Configured blog {_settings.CurrentValue.BlogUniqueId} cannot be found");

        return _context
            .Posts
            .Count(x => x.Blog == _lazyBlog.Value && 
                        (x.ShowState == ShowState.Blog || x.ShowState == ShowState.BlogAndMenu));
    }

    public IEnumerable<Post> PostsOnBlog(int currentPage)
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_settings.CurrentValue.BlogUniqueId} cannot be found");

        return _context.Posts
            .Include(x => x.HeaderImage)
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Blog || x.ShowState == ShowState.BlogAndMenu))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt)
            .Skip((currentPage - 1) * _settings.CurrentValue.PageSize)
            .Take(_settings.CurrentValue.PageSize);
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
            throw new InvalidOperationException($"Configured blog {_settings.CurrentValue.BlogUniqueId} cannot be found");

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
            throw new InvalidOperationException($"Configured blog {_settings.CurrentValue.BlogUniqueId} cannot be found");

        return _context
            .Posts
            // todo use flag and or
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowState.Footer || x.ShowState == ShowState.BlogAndFooter || x.ShowState == ShowState.LinkAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt);
    }
}