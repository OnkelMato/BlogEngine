using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Pages;

namespace OnkelMato.BlogEngine.Web;

public class BlogEngineLinkFactory(IHttpContextAccessor contextAccessor) : ILinkFactory
{
    public string CreatePostLink(Post post)
    {
        // get blogurl and port
        var blogUrl = contextAccessor.HttpContext?.Request.Scheme + "://" + contextAccessor.HttpContext?.Request.Host.Value;

        return blogUrl.TrimEnd('/') + "/Post/" + Regex.Replace(post.Title, "[^a-zA-Z0-9 ]", "").Replace(" ", "_") + "/" + post.UniqueId + "/";
    }
}