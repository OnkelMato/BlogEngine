using System.ServiceModel.Syndication;

namespace OnkelMato.BlogEngine.Core.Service;

public interface IBlogEngineRssProvider
{
    Task<SyndicationFeed> RetrieveSyndicationFeed();
}
