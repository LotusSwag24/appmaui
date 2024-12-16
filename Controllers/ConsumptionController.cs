using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sbm.Server.Interfaces;
using sbm.Shared.Dto;

namespace sbm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("sbm/consumption")]
    public class ConsumptionController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;

        public ConsumptionController(IConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }

        [AllowAnonymous]
        [HttpPost("createConsumption")]
        public async Task<string> CreateConsumption([FromBody] Consumption consumption)
        {
            var response = await _consumptionService.CreateConsumption(consumption);
            return response;
        }
    }
}
