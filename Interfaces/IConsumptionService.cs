using sbm.Shared.Dto;

namespace sbm.Server.Interfaces
{
    public interface IConsumptionService
    {
        Task<string> CreateConsumption(Consumption consumption);
    }
}
