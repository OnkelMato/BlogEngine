using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Core.Repository;

/// <summary>
/// Provides read-only access to blog data for the configured blog, including retrieval of posts, post counts, and
/// related images.
/// </summary>
/// <remarks>This repository is intended for querying blog content and does not support modification operations.
/// All methods operate within the context of the blog identified by the supplied blog identifier provider. If the
/// configured blog cannot be found, certain methods will throw an exception. This class is not thread-safe; concurrent
/// access should be managed externally if required.</remarks>
public class BlogEngineReadRepository
{
    private readonly BlogEngineContext _context;
    private readonly IBlogIdProvider _blogId;
    private readonly Lazy<BlogDb?> _lazyBlog;

    /// <summary>
    /// Initializes a new instance of the BlogEngineReadRepository class using the specified database context and blog
    /// identifier provider.
    /// </summary>
    /// <param name="context">The database context used to access blog data. Cannot be null.</param>
    /// <param name="blogId">The provider that supplies the unique identifier for the current blog.</param>
    /// <exception cref="ArgumentNullException">Thrown if the context parameter is null.</exception>
    public BlogEngineReadRepository(
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

    public Blog? Blog() => _lazyBlog.Value?.ToModel();

    /// <summary>
    /// Returns the total number of posts associated with the configured blog 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public int PostsOnBlogCount()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            .Count(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowStateDb.Blog || x.ShowState == ShowStateDb.BlogAndMenu || x.ShowState == ShowStateDb.BlogAndFooter));
    }

    /// <summary>
    /// Retrieves a paged collection of posts for the configured blog, ordered by display order and publication date.
    /// </summary>
    /// <param name="currentPage">The one-based index of the page of posts to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="postsPerPage">The maximum number of posts to include in the returned page. Must be greater than zero.</param>
    /// <returns>An enumerable collection of posts for the specified page. The collection may be empty if there are no posts for
    /// the given page.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configured blog cannot be found.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if postsPerPage is less than or equal to zero.</exception>
    public IEnumerable<Post> PostsOnBlog(int currentPage, int postsPerPage)
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        if (postsPerPage <= 0)
            throw new ArgumentOutOfRangeException(nameof(postsPerPage), "PostsPerPage must be greater than zero");

        var posts = _context.Posts
            .Include(x => x.HeaderImage)
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowStateDb.Blog || x.ShowState == ShowStateDb.BlogAndMenu || x.ShowState == ShowStateDb.BlogAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt)
            .Skip((currentPage - 1) * postsPerPage)
            .Take(postsPerPage);

        return posts.Select(x => x.ToModel());
    }

    /// <summary>
    /// Retrieves a post with the specified unique identifier for the current blog, including its header image.
    /// </summary>
    /// <param name="id">The unique identifier of the post to retrieve.</param>
    /// <returns>A <see cref="Post"/> object representing the post with the specified identifier, or <see langword="null"/> if no
    /// matching post is found.</returns>
    public async Task<Post?> PostAsync(Guid id)
    {
        var result = await _context.Posts
            .Include(x => x.HeaderImage)
            .Include(x => x.PostTags)
            .SingleOrDefaultAsync(x => x.Blog == _lazyBlog.Value && x.UniqueId == id);

        return result?.ToModel();
    }

    /// <summary>
    /// Retrieves all posts in the current blog that are configured to appear in the menu.
    /// </summary>
    /// <returns>An enumerable collection of posts that are marked to be shown in the menu, ordered by their display order and
    /// publication date.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configured blog cannot be found.</exception>
    public IEnumerable<Post> PostsInMenu()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            .Include(x => x.PostTags)
            .Where(x => x.Blog == _lazyBlog.Value && (x.ShowState == ShowStateDb.Menu || x.ShowState == ShowStateDb.BlogAndMenu || x.ShowState == ShowStateDb.LinkAndMenu))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt)
            .Select(x => x.ToModel()).ToArray();
    }

    /// <summary>
    /// Retrieves the collection of posts that are configured to appear in the footer for the current blog.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Post"/> objects that are displayed in the footer. The collection is
    /// ordered by the post's display order and publication date.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configured blog cannot be found.</exception>
    public IEnumerable<Post> PostsInFooter()
    {
        if (_lazyBlog.Value == null)
            throw new InvalidOperationException($"Configured blog {_blogId.Id} cannot be found");

        return _context
            .Posts
            .Include(x => x.PostTags)
            .Where(x => x.Blog == _lazyBlog.Value && (x.ShowState == ShowStateDb.Footer || x.ShowState == ShowStateDb.BlogAndFooter || x.ShowState == ShowStateDb.LinkAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt)
            .Select(x => x.ToModel()).ToArray();
    }

    /// <summary>
    /// Retrieves the image associated with the specified unique identifier for the current blog.
    /// </summary>
    /// <param name="imageId">The unique identifier of the image to retrieve.</param>
    /// <returns>A <see cref="PostImage"/> object representing the image if found; otherwise, <see langword="null"/>.</returns>
    [Obsolete("use PostImageAsync() instead")]
    public PostImage? GetImage(Guid imageId)
    {
        return _context.PostImages.SingleOrDefault(x => x.UniqueId == imageId && x.Blog == _lazyBlog.Value)?.ToModel();
    }

    /// <summary>
    /// Asynchronously retrieves the image associated with the specified unique identifier for the current blog.
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns>A <see cref="PostImage"/> object representing the image if found; otherwise, <see langword="null"/>.</returns>
    public async Task<PostImage?> PostImageAsync(Guid imageId)
    {
        return (await _context.PostImages.SingleOrDefaultAsync(x => x.UniqueId == imageId && x.Blog == _lazyBlog.Value))?.ToModel();
    }

    public async Task<Blog?> GetEntireBlog()
    {
        var result = await _context.Blogs
            .Include(x => x.Posts).ThenInclude(x => x.HeaderImage)
            .Include(x => x.PostImages)
            .Include(x => x.PostTags)
            .FirstOrDefaultAsync(m => m.UniqueId == _lazyBlog.Value!.UniqueId);
        return result?.ToModel();
    }

    public IList<PostImage> GetAllImages()
    {
        var result = _context.PostImages
            .Where(x => x.Blog == _lazyBlog.Value)
            .OrderByDescending(x => x.UpdatedAt);

        return result.Select(x => x.ToModel()).ToList();
    }

    public async Task<IEnumerable<Post>> GetAllPosts()
    {
        var posts = _context.Posts
            .Include(x => x.HeaderImage)
            .Include(x => x.PostTags)
            .Where(x => x.Blog == _lazyBlog.Value &&
                        (x.ShowState == ShowStateDb.Blog || x.ShowState == ShowStateDb.BlogAndMenu ||
                         x.ShowState == ShowStateDb.BlogAndFooter))
            .OrderBy(x => x.Order).ThenByDescending(x => x.PublishedAt);

        return posts.Select(x => x.ToModel());
    }
}