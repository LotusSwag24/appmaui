using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sbm.Server.Interfaces;
using sbm.Shared.Dto.Catalogs;

namespace sbm.Server.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("sbm/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService userCompanyService)
        {
            _catalogService = userCompanyService;
        }

        [HttpGet("getAllCompaniesByUserId/{userId}")]
        public async Task<List<Company>> GetAllCompaniesByUserIdAsync(string userId)
        {
            var response = await _catalogService.GetAllCompaniesByUserIdAsync(userId).ConfigureAwait(true);
            return response;
        }

        [HttpPost("populate-catalogs-from-sap")]
        public async Task PopulateAllCatalogsAsync()
        {
            await _catalogService.PopulateAllCatalogsAsync().ConfigureAwait(true);
        }


        [HttpGet("getManufactoringOrdersByCompanyId/{companyId}")]
        public async Task<List<ManufactoringOrder>> GetManufactoringOrdersByCompanyIdAsync(string companyId)
        {
            var response = await _catalogService.GetManufacturingOrdersByCompanyIdAsync(companyId).ConfigureAwait(true);
            return response;
        }


        [HttpGet("getArticlesByCompanyId/{companyId}")]
        public async Task<List<Article>> GetArticlesByCompanyIdAsync(string companyId)
        {
            var response = await _catalogService.GetArticlesByCompanyIdAsync(companyId).ConfigureAwait(true);
            return response;
        }

    }
}
