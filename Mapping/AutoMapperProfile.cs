using AutoMapper;
using sbm.Shared.Dto;
using sbm.Shared.Dto.Catalogs;
using sbm.Shared.Sap;

namespace sbm.Server.Mapping
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            #region Catalogs Sap to SBM
            CreateMap<Shared.Sap.VersionCatalogo, Data.Entities.Catalogs.CatalogVersion>()
            .ForMember(dest => dest.CatalogName, opt => opt.MapFrom(src => src.Catalogo))
            .ForMember(dest => dest.LastVersion, opt => opt.MapFrom(src => src.UltimaVersion));


            CreateMap<Empresa, Data.Entities.Catalogs.Company>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.Ruc, opt => opt.MapFrom(src => src.Ruc))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Nombre));

            CreateMap<Data.Entities.Catalogs.Company, Company>().ReverseMap();

            CreateMap<Articulo, Data.Entities.Catalogs.Article>()
               .ForMember(dest => dest.ArticleCodeId, opt => opt.MapFrom(src => src.codigoArticulo))
               .ForMember(dest => dest.ArticleName, opt => opt.MapFrom(src => src.nombreArticulo))
               .ForMember(dest => dest.ManufacturerSerialNumber, opt => opt.MapFrom(src => src.numSerieFabricante))
               .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.numSerie))
               .ForMember(dest => dest.WareHouseCode, opt => opt.MapFrom(src => src.codBodega));

            CreateMap<Data.Entities.Catalogs.Article, Article>().ReverseMap();

            CreateMap<OrdenesFab, Data.Entities.Catalogs.ManufactoringOrder>()
            .ForMember(dest => dest.DocEntry, opt => opt.MapFrom(src => src.docEntry))
            .ForMember(dest => dest.ManufactoringOrderNumber, opt => opt.MapFrom(src => src.numeroOf))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.codBodega))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.bodega))
            .ForMember(dest => dest.Container, opt => opt.MapFrom(src => src.contenedor))
            .ForMember(dest => dest.Week, opt => opt.MapFrom(src => src.semana))
            .ForMember(dest => dest.ManufactoringOrderDetails, opt => opt.MapFrom(src => src.detalle));

            CreateMap<Data.Entities.Catalogs.ManufactoringOrder, ManufactoringOrder>()
                .ForMember(dest => dest.ManufactoringOrderDetails, opt => opt.MapFrom(src => src.ManufactoringOrderDetails));

            //CreateMap<ManufactoringOrder, Data.Entities.Catalogs.ManufactoringOrder>()
            //    .ForMember(dest => dest.ManufactoringOrderDetails, opt => opt.MapFrom(src => src.ManufactoringOrderDetails));

            CreateMap<Detalle, Data.Entities.Catalogs.ManufactoringOrderDetail>()
           .ForMember(dest => dest.NumberLine, opt => opt.MapFrom(src => src.numLinea))
           .ForMember(dest => dest.ArticleCode, opt => opt.MapFrom(src => src.codArticulo))
           .ForMember(dest => dest.ArticleName, opt => opt.MapFrom(src => src.articulo))
           .ForMember(dest => dest.PlannedQuantity, opt => opt.MapFrom(src => src.cantidadPlanificada))
           .ForMember(dest => dest.StoreCode, opt => opt.MapFrom(src => src.codAlamcenArticulo));

            CreateMap<Data.Entities.Catalogs.ManufactoringOrderDetail, ManufactoringOrderDetail>();

            CreateMap<ManufactoringOrderDetail, Data.Entities.Catalogs.ManufactoringOrderDetail>();

            #endregion


            CreateMap<Data.Entities.User, User>().ReverseMap();
            CreateMap<Data.Entities.Catalogs.Company, ManufactoringOrder>().ReverseMap();
            CreateMap<Data.Entities.UserCompany, UserCompany>().ReverseMap();
            CreateMap<Data.Entities.Consumption, Consumption>().ReverseMap();
            CreateMap<Data.Entities.ConsumptionDetail, ConsumptionDetail>().ReverseMap();

            //Mapeo ManufacturingOrde a Consumption
            //CreateMap<ManufacturingOrder, Consumption>()
            //    .ForMember(dest => dest.ManufacturingOrderNumber, opt => opt.MapFrom(src => src.ManufactoringOrderNumber))
            //    .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.WarehouseCode))
            //    .ForMember(dest => dest.ContainerNumber, opt => opt.MapFrom(src => src.Container))
            //    .ForMember(dest => dest.WeekId, opt => opt.MapFrom(src => src.Week))
            //    .ForMember(dest => dest.ConsumptionDetails, opt => opt.MapFrom(src => src.ManufactoringOrderDetails));

            //CreateMap<ManufacturingOrderDetail, ConsumptionDetail>()
            //    .ForMember(dest => dest.ConsumptionId, opt => opt.MapFrom(src => src.ManufactoringOrderId))
            //    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.PlannedQuantity));
        }
    }
}
