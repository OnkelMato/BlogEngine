using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Web;

public class BlogEngineLinkFactory(IHttpContextAccessor contextAccessor) : ILinkFactory
{
    public string CreatePostLink(Post post)
    {
        // get blog url and port
        var blogUrl = contextAccessor.HttpContext?.Request.Scheme + "://" + contextAccessor.HttpContext?.Request.Host.Value;

        return blogUrl.TrimEnd('/') + "/Post/" + GetSEOTitle(post.Title) + "/" + post.UniqueId + "/";
    }

    public string GetSEOTitle(string title)
    {
        return Regex.Replace(title, "[^a-zA-Z0-9 ]", "").Replace(" ", "_");
    }
}