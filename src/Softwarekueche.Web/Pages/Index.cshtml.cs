using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Softwarekueche.Web.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Softwarekueche.Web.Pages;

public class IndexModel(ILogger<IndexModel> logger, SoftwarekuecheHomeContext context, IOptions<PostsConfiguration> postsConfiguration)
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

    }

    private readonly ILogger<IndexModel> _logger = logger;
    private readonly SoftwarekuecheHomeContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly PostsConfiguration _postsConfiguration = postsConfiguration.Value ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostModel> Posts { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int Skip { get; set; } = 0;

    public void OnGet()
    {
        Posts = _context.Posts
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip(Skip * _postsConfiguration.PageSize)
            .Take(_postsConfiguration.PageSize)
            .Select(x => new PostModel()
        {
            Title = x.Title,
            UniqueId = x.UniqueId,
            UpdatedAt = x.UpdatedAt,
            HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null),
            DetailLink = Url.Page("/Detail", null, new { id = x.UniqueId }, Request.Scheme) ?? string.Empty,
        }).ToList();
    }
}