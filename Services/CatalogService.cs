using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using sbm.Data.Contexts;
using sbm.Data.Interfaces;
using sbm.Data.Interfaces.Catalogs;
using sbm.Data.Repositories;
using sbm.Data.Repositories.Catalogs;
using sbm.Server.Interfaces;
using sbm.Shared.Dto.Catalogs;
using sbm.Shared.Exceptions;
using sbm.Shared.Sap;
using System.Net;

namespace sbm.Server.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<SbmContext> _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISapService _sapService;

        public CatalogService(IMapper mapper, IDbContextFactory<SbmContext> contextFactory, IHttpContextAccessor httpContextAccessor, ISapService sapService)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
            _sapService = sapService;
        }

        public async Task<List<Company>> GetAllCompaniesByUserIdAsync(string userId)
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();

                var user = await unitOfWork.Repository<IUserRepository, UserRepository>().GetUserByUserIdAsync(userId).ConfigureAwait(true);
                var companies = user.UserCompanyList.Select(uc => uc.Company).ToList();

                var companyList = _mapper.Map<List<Company>>(companies);
                return companyList;
            }
            catch (Exception ex)
            {
                var friendlyMessage = "Lamentamos los inconvenientes, por favor intente de nuevo.";
                var httpStatusCode = (int)HttpStatusCode.InternalServerError;
                throw new HttpException(ex.Message, friendlyMessage, httpStatusCode, ex.InnerException);
            }
        }

        public async Task PopulateAllCatalogsAsync()
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                var companies = await unitOfWork.Repository<ICompanyRepository, CompanyRepository>().ListAllAsync().ConfigureAwait(true);

                foreach (var company in companies)
                {
                    var catalogsVersions = await _sapService.RetrieveCatalogInformationAsync<VersionCatalogo, Data.Entities.Catalogs.CatalogVersion>($"{EndPointPath.RetrieveCatalogsAsync}?empresa={company.Id}", true)
                    .ConfigureAwait(true);
                    catalogsVersions.ForEach(cv => cv.LastUpdate = DateTime.Now);
                    var catalogName = "OF";
                    if (catalogsVersions.Any(cv => string.Compare(cv.CatalogName, catalogName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        var newCatalogVersion = catalogsVersions
                            .FirstOrDefault(cv => string.Compare(cv.CatalogName, catalogName, StringComparison.Ordinal) == 0);
                        await PopulateManufacturingOrderCatalogAsync(newCatalogVersion, company.Id).ConfigureAwait(false);
                    }

                    catalogName = "SERIES";
                    if (catalogsVersions.Any(cv => string.Compare(cv.CatalogName, catalogName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        var newCatalogVersion = catalogsVersions
                            .FirstOrDefault(cv => string.Compare(cv.CatalogName, catalogName, StringComparison.Ordinal) == 0);
                        await PopulateArticleCatalogAsync(newCatalogVersion, company.Id).ConfigureAwait(false);
                    }
                }
            }
            catch (HttpException ex)
            {
                unitOfWork.Rollback();
                throw;
            }
            catch (SqlException sqlEx)
            {
                unitOfWork.Rollback();
                throw new HttpException(sqlEx.Message, "No se pudo poblar el catálogo de semanas.",
                    (int)HttpStatusCode.InternalServerError, sqlEx.InnerException);
            }
        }

        private async Task PopulateCompaniesCatalogAsync()
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();
                //var oldCatalogVersion = await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>()
                //    .GetEntityByIdAsync(newCatalogVersion.CatalogName).ConfigureAwait(true);
                //if (oldCatalogVersion == null || oldCatalogVersion.LastVersion != newCatalogVersion.LastVersion)
                //{                  
                var companies = await _sapService.RetrieveCatalogInformationAsync<Empresa, Data.Entities.Catalogs.Company>(EndPointPath.GetCompanies)
                .ConfigureAwait(true);

                await unitOfWork.Repository<ICompanyRepository, CompanyRepository>().PopulateCompaniesAsync(companies).ConfigureAwait(true);

                //await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>().UpsertAsync(newCatalogVersion).ConfigureAwait(true);
                //}
                unitOfWork.Commit();
            }
            catch (HttpException)
            {
                unitOfWork.Rollback();
                throw;
            }
            catch (SqlException sqlEx)
            {
                unitOfWork.Rollback();
                throw new HttpException(sqlEx.Message, "No se pudo poblar el catálogo de semanas.",
                    (int)HttpStatusCode.InternalServerError, sqlEx.InnerException);
            }
        }

        private async Task PopulateArticleCatalogAsync(Data.Entities.Catalogs.CatalogVersion newCatalogVersion, string companyId)
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();
                var oldCatalogVersion = await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>()
                    .GetEntityByIdAsync(newCatalogVersion.CatalogName).ConfigureAwait(true);
                if (oldCatalogVersion == null || oldCatalogVersion.LastVersion != newCatalogVersion.LastVersion)
                {
                    var articles = await _sapService.RetrieveCatalogInformationAsync<Articulo, Data.Entities.Catalogs.Article>($"{EndPointPath.GetArticles}?empresa={companyId}")
                        .ConfigureAwait(true);
                    await unitOfWork.Repository<IArticleRepository, ArticleRepository>().PopulateArticlesByCompanyIdAsync(companyId, articles).ConfigureAwait(true);

                    await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>().UpsertAsync(newCatalogVersion).ConfigureAwait(true);
                }
                unitOfWork.Commit();
            }
            catch (HttpException)
            {
                unitOfWork.Rollback();
                throw;
            }
            catch (SqlException sqlEx)
            {
                unitOfWork.Rollback();
                throw new HttpException(sqlEx.Message, "No se pudo poblar el catálogo de semanas.",
                    (int)HttpStatusCode.InternalServerError, sqlEx.InnerException);
            }
        }

        private async Task PopulateManufacturingOrderCatalogAsync(Data.Entities.Catalogs.CatalogVersion newCatalogVersion, string companyId)
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();
                var oldCatalogVersion = await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>()
                    .GetEntityByIdAsync(newCatalogVersion.CatalogName).ConfigureAwait(true);
                if (oldCatalogVersion == null || oldCatalogVersion.LastVersion != newCatalogVersion.LastVersion)
                {
                    var manufacturingOrders = await _sapService.RetrieveCatalogInformationAsync<OrdenesFab, Data.Entities.Catalogs.ManufactoringOrder>($"{EndPointPath.GetManufacturingOrders}?empresa={companyId}&semana=49")
                        .ConfigureAwait(true);
                    await unitOfWork.Repository<IManufactoringOrderRepository, ManufactoringOrderRepository>().PopulateManufactoringOrdersAsync(companyId, manufacturingOrders).ConfigureAwait(true);

                    await unitOfWork.Repository<ICatalogVersionRepository, CatalogVersionRepository>().UpsertAsync(newCatalogVersion).ConfigureAwait(true);

                }
                unitOfWork.Commit();
            }
            catch (HttpException)
            {
                unitOfWork.Rollback();
                throw;
            }
            catch (SqlException sqlEx)
            {
                unitOfWork.Rollback();
                throw new HttpException(sqlEx.Message, "No se pudo poblar el catálogo de ordenes de fabricación.",
                    (int)HttpStatusCode.InternalServerError, sqlEx.InnerException);
            }
        }

        public async Task<List<ManufactoringOrder>> GetManufacturingOrdersByCompanyIdAsync(string companyId)
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();

                var manufactoringOrders = await unitOfWork.Repository<IManufactoringOrderRepository, ManufactoringOrderRepository>()
                    .RetrieveManufactoringOrderByCompanyId(companyId).ConfigureAwait(true);

                var x = _mapper.Map<List<ManufactoringOrder>>(manufactoringOrders);
                return x;
            }
            catch (Exception ex)
            {
                var friendlyMessage = "Lamentamos los inconvenientes, por favor intente de nuevo.";
                var httpStatusCode = (int)HttpStatusCode.InternalServerError;
                throw new HttpException(ex.Message, friendlyMessage, httpStatusCode, ex.InnerException);
            }
        }

        public async Task<List<Article>> GetArticlesByCompanyIdAsync(string companyId)
        {
            using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
            try
            {
                unitOfWork.BeginTransaction();

                var articles = await unitOfWork.Repository<IArticleRepository, ArticleRepository>().RetrieveArticlesByCompanyId(companyId).ConfigureAwait(true);
                var response = _mapper.Map<List<Article>>(articles);
                return response;
            }
            catch (Exception ex)
            {
                var friendlyMessage = "Lamentamos los inconvenientes, por favor intente de nuevo.";
                var httpStatusCode = (int)HttpStatusCode.InternalServerError;
                throw new HttpException(ex.Message, friendlyMessage, httpStatusCode, ex.InnerException);
            }
        }

    }
}
