using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Body.Services;
using MyHealth.API.Body.Validators;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Functions
{
    public class GetWeightLogByDate
    {
        private readonly IBodyDbService _bodyDbService;
        private readonly IDateValidator _dateValidator;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetWeightLogByDate(
            IBodyDbService bodyDbService,
            IDateValidator dateValidator,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _bodyDbService = bodyDbService ?? throw new ArgumentNullException(nameof(bodyDbService));
            _serviceBusHelpers = serviceBusHelpers ?? throw new ArgumentNullException(nameof(serviceBusHelpers));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dateValidator = dateValidator ?? throw new ArgumentNullException(nameof(dateValidator));
        }

        [FunctionName(nameof(GetWeightLogByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Weight/{weightDate}")] HttpRequest req,
            ILogger log,
            string weightDate)
        {
            IActionResult result;

            try
            {
                bool isDateValid = _dateValidator.IsWeightLogDateValid(weightDate);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                var weightResponse = await _bodyDbService.GetWeightRecordByDate(weightDate);
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
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
