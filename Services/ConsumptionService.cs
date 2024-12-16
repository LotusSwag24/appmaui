using Audit.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sbm.Data.Contexts;
using sbm.Data.Interfaces;
using sbm.Data.Repositories;
using sbm.Server.Interfaces;
using sbm.Shared;
using sbm.Shared.Dto;
using sbm.Shared.Exceptions;
using System.Net;

namespace sbm.Server.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<SbmContext> _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ConsumptionService(IMapper mapper, IDbContextFactory<SbmContext> contextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CreateConsumption(Consumption consumption)
        {
            var response = "";
            try
            {
                if (consumption.Status == ConsumptionStatus.New)
                {
                        Data.Entities.Consumption createdConsumption = null;
                        response = string.Empty;

                        using IUnitOfWork<SbmContext> unitOfWork = new UnitOfWork<SbmContext>(_contextFactory);
                        try
                        {
                            var consumptionToCreate = _mapper.Map<Data.Entities.Consumption>(consumption);
                            consumptionToCreate.Id = 0;
                            consumptionToCreate.CreatedOn = DateTime.Now;
                            consumptionToCreate.ModifiedOn = DateTime.Now;
                            consumptionToCreate.ConsumptionDetails.ToList().ForEach(x => { x.Id = 0; x.CreatedOn = DateTime.Now; x.ModifiedOn = DateTime.Now; });

                            createdConsumption = (await unitOfWork.Repository<IConsumptionRepository, ConsumptionRepository>()
                                .AddAsync(consumptionToCreate).ConfigureAwait(false)).Single();
                            response = $"Consumo creado con éxito. Id: {createdConsumption.Id}";
                        }
                        catch (HttpException httpEx)
                        {
                            response = $"El consumo no pudo ser creado por el siguiente motivo: {httpEx.FriendlyMessage}.";
                            Console.WriteLine(response);
                        }
                        catch (Exception ex)
                        {
                            response = $"El consumo no pudo ser creado por el siguiente motivo: {ex.Message}.";
                            Console.WriteLine(response);
                        }
                    }
            }
            catch (Exception ex)
            {
                var friendlyMessage = "Ocurrió un error al intentar crear el consumo, por favor intenelo de nuevo";
                var httpStatusCode = (int)HttpStatusCode.InternalServerError;
                throw new HttpException(ex.Message, friendlyMessage, httpStatusCode, ex.InnerException);
            }
            return response;
        }
    }
}

