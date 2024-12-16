
namespace sbm.Server.Interfaces
{
    public interface ISapService
    {
        Task CallSapApiAsync(string relativeUri, HttpMethod httpMethod = null, bool isCatalogVersion = false);
        Task<TOutput> CallSapApiAsync<TOutput>(string relativeUri, HttpMethod httpMethod = null, bool isCatalogVersion = false) where TOutput : class;
        Task CallSapApiAsync<TInput>(string relativeUri, HttpMethod httpMethod, TInput body, bool isCatalogVersion) where TInput : class;
        Task<TOutput> CallSapApiAsync<TInput, TOutput>(string relativeUri, HttpMethod httpMethod, bool isCatalogVersion = false, TInput body = null)
            where TInput : class
            where TOutput : class;

        Task<List<TSbmEntity>> RetrieveCatalogInformationAsync<TSapEntity, TSbmEntity>(string endpointPath, bool isCatalogVersion = false)
            where TSapEntity : class
            where TSbmEntity : Data.Interfaces.IEntity;
        Task<List<TSapEntity>> RetrieveCatalogInformationAsync<TSapEntity>(string endpointPath, bool isCatalogVersion = false);
    }
}
