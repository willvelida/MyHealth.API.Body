using MyHealth.API.Body.Repository.Interfaces;
using MyHealth.API.Body.Services.Interfaces;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Services
{
    public class BodyService : IBodyService
    {
        private readonly IBodyRepository _bodyRepository;

        public BodyService(IBodyRepository bodyRepository)
        {
            _bodyRepository = bodyRepository;
        }

        public async Task<WeightEnvelope> GetWeightRecordByDate(string weightLogDate)
        {
            try
            {
                var weightEnvelope = await _bodyRepository.ReadWeightRecordByDate(weightLogDate);
                return weightEnvelope;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<WeightEnvelope>> GetWeightRecords()
        {
            try
            {
                var weightEnvelopes = await _bodyRepository.ReadAllWeightRecords();
                return weightEnvelopes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsWeightLogDateValid(string weightLogDate)
        {
            bool isDateValid = false;
            string pattern = "yyyy-MM-dd";
            DateTime parsedWeightLogDate;

            if (DateTime.TryParseExact(weightLogDate, pattern, null, DateTimeStyles.None, out parsedWeightLogDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}
