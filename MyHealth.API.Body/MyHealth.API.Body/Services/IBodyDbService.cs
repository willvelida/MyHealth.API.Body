using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.Services
{
    public interface IBodyDbService
    {
        /// <summary>
        /// Retrieves all weight logs.
        /// </summary>
        /// <returns></returns>
        Task<List<mdl.WeightEnvelope>> GetWeightRecords();
        /// <summary>
        /// Retrieves a weight log for a specific date
        /// </summary>
        /// <param name="weightLogDate"></param>
        /// <returns></returns>
        Task<mdl.WeightEnvelope> GetWeightRecordByDate(string weightLogDate);
    }
}
