using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Body.Services.Interfaces;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Functions
{
    public class GetWeightLogByDate
    {
        private readonly IBodyService _bodyService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GetWeightLogByDate> _logger;

        public GetWeightLogByDate(
            IBodyService bodyService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration,
            ILogger<GetWeightLogByDate> logger)
        {
            _bodyService = bodyService;
            _serviceBusHelpers = serviceBusHelpers;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName(nameof(GetWeightLogByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Weight/{weightDate}")] HttpRequest req,
            string weightDate)
        {
            IActionResult result;

            try
            {
                bool isDateValid = _bodyService.IsWeightLogDateValid(weightDate);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                var weightResponse = await _bodyService.GetWeightRecordByDate(weightDate);
                if (weightResponse == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                var weight = weightResponse.Weight;
                result = new OkObjectResult(weight);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
