using System.ComponentModel.DataAnnotations;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine;
using OnkelMato.BlogEngine.Database;

namespace Softwarekueche.Web.Pages;

public class DetailModel(ILogger<DetailModel> logger, BlogEngineContext context, IOptions<PostsConfiguration> postsConfiguration) : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string HtmlPreview { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime UpdatedAt { get; set; }

        public Guid? HeaderImage { get; set; }
    }

    private readonly ILogger<DetailModel> _logger = logger;
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly PostsConfiguration _postsConfiguration = postsConfiguration.Value ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public PostModel Post { get; set; } = new PostModel();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public void OnGet()
    {
        Post = _context.Posts
            .Include(x => x.HeaderImage)
            .Where(x => x.UniqueId == Id).Select(x => new PostModel()
            {
                Title = x.Title,
                UniqueId = x.UniqueId,
                UpdatedAt = x.UpdatedAt,
                HeaderImage = x.HeaderImage == null ? null : x.HeaderImage.UniqueId,
                HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null),
                HtmlContent = Markdown.ToHtml(x.MdContent ?? string.Empty, null, null)
            }).Single();
    }
}

