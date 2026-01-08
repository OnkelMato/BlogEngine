using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Pages;

public interface ILinkFactory
{
    string CreatePostLink(Post post);
}