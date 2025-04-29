﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Web.ViewComponents;

public class BlogFooterViewComponent(BlogEngineRepository repository, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : ViewComponent
{
    private readonly BlogEngineRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public List<DynamicMenuModel> MenuPosts { get; set; } = [];

    public IViewComponentResult Invoke(int maxPriority, bool isDone)
    {
        MenuPosts = _repository.PostsInFooter()
            .Select(x => new DynamicMenuModel()
            {
                PostId = x.UniqueId,
                Url = x.ShowState == ShowState.LinkAndFooter ? x.MdPreview : null, // because MdPreview is required
                Title = x.Title
            }).ToList();

        return View(this);
    }

    public class DynamicMenuModel
    {
        public string Title { get; set; } = null!;
        public string? Url { get; set; } = null!;
        public Guid? PostId { get; set; }
    }
}