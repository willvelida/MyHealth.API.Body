using MyHealth.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Repository.Interfaces
{
    public interface IBodyRepository
    {
        Task<List<WeightEnvelope>> ReadAllWeightRecords();
        Task<WeightEnvelope> ReadWeightRecordByDate(string weightLogDate);
    }
}
