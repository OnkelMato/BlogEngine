using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Construction;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Web.Pages;

public class IndexModel(BlogEngineRepository repository, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleSEO { get; set; } = string.Empty;
        public string HtmlPreview { get; set; } = string.Empty;
        public bool HasContent { get; set; }
        public Guid? HeaderImage { get; set; }

        public DateTime PublishedAt { get; set; }
    }

    private readonly BlogEngineRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostModel> Posts { get; set; } = [];

    public bool AllowBlogAdministration => _postsConfiguration.CurrentValue.AllowBlogAdministration;
    
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public int NumOfPages { get; set; }

    public IActionResult OnGet()
    {
        // load blog data
        var blog = _repository.Blog();
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        this.Title = blog.Title;
        this.Description = blog.Description;

        // load posts
        var numOfPosts = _repository.PostsOnBlogCount();
        // cf: https://stackoverflow.com/questions/4846493/how-to-always-round-up-to-the-next-integer
        NumOfPages = (numOfPosts + (_postsConfiguration.CurrentValue.PageSize - 1)) / _postsConfiguration.CurrentValue.PageSize;

        Posts = _repository.PostsOnBlog(CurrentPage)
            .Select(x => new PostModel()
            {
                Title = x.Title,
                TitleSEO = Regex.Replace(x.Title, "[^a-zA-Z0-9 ]", "").Replace(" ", "_"),
                UniqueId = x.UniqueId,
                UpdatedAt = x.UpdatedAt,
                PublishedAt = x.PublishedAt,
                HasContent = x.MdContent != null,
                HeaderImage = x.HeaderImage?.UniqueId,
                //HtmlPreview = Markdown.ToHtml(WebUtility.HtmlEncode(x.MdPreview), null, null)
                HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null)
            }).ToList();

        return Page();
    }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}