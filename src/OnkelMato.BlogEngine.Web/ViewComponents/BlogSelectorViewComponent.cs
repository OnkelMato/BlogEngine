using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Web.ViewComponents;

public class BlogSelectorViewComponent(BlogEngineMgmtRepository repository, IOptionsMonitor<BlogConfiguration> blogConfiguration) : ViewComponent
{
    public List<BlogSelectorModel> MenuBlogs { get; set; } = [];
    public bool AllowBlogSelection => blogConfiguration.CurrentValue.EnableBlogSelection;

    public IViewComponentResult Invoke(int maxPriority, bool isDone)
    {
        MenuBlogs = repository.GetAllBlogs()
            .Select(x => new BlogSelectorModel()
            {
                BlogId = x.UniqueId,
                Title = x.Title
            }).ToList();

        return View(this);
    }

    public class BlogSelectorModel
    {
        public string Title { get; set; } = null!;
        public Guid BlogId { get; set; }
    }

}
