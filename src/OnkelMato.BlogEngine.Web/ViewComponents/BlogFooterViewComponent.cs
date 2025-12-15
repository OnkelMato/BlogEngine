using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Web.ViewComponents;

public class BlogFooterViewComponent(BlogEngineReadRepository repository, IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : ViewComponent
{
    private readonly BlogEngineReadRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public List<DynamicMenuModel> MenuPosts { get; set; } = [];

    public IViewComponentResult Invoke(int maxPriority, bool isDone)
    {
        MenuPosts = _repository.PostsInFooter()
            .Select(x => new DynamicMenuModel()
            {
                PostId = x.UniqueId,
                Url = x.ShowState == ShowState.LinkAndFooter ? x.MdPreview : null, // because MdPreview is required
                TitleSEO = Regex.Replace(x.Title, "[^a-zA-Z0-9 ]", "").Replace(" ", "_"),
                Title = x.Title
            }).ToList();

        return View(this);
    }

    public class DynamicMenuModel
    {
        public string Title { get; set; } = null!;
        public string? Url { get; set; } = null!;
        public Guid? PostId { get; set; }
        public string? TitleSEO { get; set; }
    }
}