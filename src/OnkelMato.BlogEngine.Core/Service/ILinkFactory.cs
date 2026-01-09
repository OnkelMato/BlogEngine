using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Core.Service;

public interface ILinkFactory
{
    string CreatePostLink(Post post);
    string GetSEOTitle(string title);
}