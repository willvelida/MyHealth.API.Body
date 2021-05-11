using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Body.Services;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.Functions
{
    public class GetAllWeightLogs
    {
        private readonly IBodyDbService _bodyDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetAllWeightLogs(
            IBodyDbService bodyDbService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _bodyDbService = bodyDbService ?? throw new ArgumentNullException(nameof(bodyDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [FunctionName(nameof(GetAllWeightLogs))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Weight")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;

            try
            {
                List<mdl.Weight> weights = new List<mdl.Weight>();

                var weightResponses = await _bodyDbService.GetWeightRecords();

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
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
