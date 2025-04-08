using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine;
using OnkelMato.BlogEngine.Database;

namespace Softwarekueche.Web.Pages;

public class IndexModel(ILogger<IndexModel> logger, BlogEngineContext context, IOptions<PostsConfiguration> postsConfiguration)
    : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlPreview { get; set; } = string.Empty;
        public string DetailLink { get; set; } = string.Empty;
        public bool HasContent { get; set; }
    }

    private readonly ILogger<IndexModel> _logger = logger;
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly PostsConfiguration _postsConfiguration = postsConfiguration.Value ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostModel> Posts { get; set; } = [];

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public int NumOfPages { get; set; }

    public void OnGet()
    {
        NumOfPages = (_context.Posts.Count() / _postsConfiguration.PageSize) + 1;

        Posts = _context.Posts
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((CurrentPage - 1) * _postsConfiguration.PageSize)
            .Take(_postsConfiguration.PageSize)
            .Select(x => new PostModel()
            {
                Title = x.Title,
                UniqueId = x.UniqueId,
                UpdatedAt = x.UpdatedAt,
                HasContent = x.MdContent != null,
                HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null),
                DetailLink = Url.Page("/Detail", null, new { id = x.UniqueId }, Request.Scheme) ?? string.Empty,
            }).ToList();
    }

}