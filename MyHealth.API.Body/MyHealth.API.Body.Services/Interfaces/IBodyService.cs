using MyHealth.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Services.Interfaces
{
    public interface IBodyService
    {
        bool IsWeightLogDateValid(string weightLogDate);
        Task<List<WeightEnvelope>> GetWeightRecords();
        Task<WeightEnvelope> GetWeightRecordByDate(string weightLogDate);
    }
}
