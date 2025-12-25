using System;
using System.Collections.Generic;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Web.Pages;

public class IndexModel(BlogEngineReadRepository repository, IBlogIdProvider blogId, IOptionsMonitor<BlogConfiguration> postsConfiguration)
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

    private readonly BlogEngineReadRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostModel> Posts { get; set; } = [];

    public bool AllowBlogAdministration => _postsConfiguration.CurrentValue.AllowAdministration;

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public int NumOfPages { get; set; }

    public IActionResult OnGet()
    {
        // load blog data
        var blog = _repository.Blog();
        if (blog == null) { return NotFound($"Blog {blogId.Id} not Found"); }

        this.Title = blog.Title;

        this.Description = blog.Description;

        // load posts
        var numOfPosts = _repository.PostsOnBlogCount();
        // cf: https://stackoverflow.com/questions/4846493/how-to-always-round-up-to-the-next-integer
        NumOfPages = (numOfPosts + (_postsConfiguration.CurrentValue.PostsPerPage - 1)) / _postsConfiguration.CurrentValue.PostsPerPage;

        Posts = _repository.PostsOnBlog(CurrentPage, _postsConfiguration.CurrentValue.PostsPerPage)
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