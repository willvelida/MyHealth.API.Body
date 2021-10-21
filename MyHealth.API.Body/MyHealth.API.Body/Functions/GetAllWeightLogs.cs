using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Body.Services.Interfaces;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.Functions
{
    public class GetAllWeightLogs
    {
        private readonly IBodyService _bodyService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GetAllWeightLogs> _logger;

        public GetAllWeightLogs(
            IBodyService bodyService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration,
            ILogger<GetAllWeightLogs> logger)
        {
            _bodyService = bodyService;
            _serviceBusHelpers = serviceBusHelpers;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName(nameof(GetAllWeightLogs))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Weight")] HttpRequest req)
        {
            IActionResult result;

            try
            {
                List<mdl.Weight> weights = new List<mdl.Weight>();

                var weightResponses = await _bodyService.GetWeightRecords();

                if (weightResponses == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                foreach (var item in weightResponses)
                {
                    weights.Add(item.Weight);
                }

                result = new OkObjectResult(weights);
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
