using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnkelMato.BlogEngine.Core.Service;

public interface ISeoProvider
{
    void SetSiteInfo(IHtmlHelper html, string siteTitle, string siteDescription, string robots = "index, follow");
}