using sbm.Shared.Dto.Catalogs;

namespace sbm.Server.Interfaces
{
    public interface ICatalogService
    {
        Task<List<Company>> GetAllCompaniesByUserIdAsync(string userId);
        Task<List<Article>> GetArticlesByCompanyIdAsync(string companyId);
        Task<List<ManufactoringOrder>> GetManufacturingOrdersByCompanyIdAsync(string companyId);
        Task PopulateAllCatalogsAsync();
    }
}
