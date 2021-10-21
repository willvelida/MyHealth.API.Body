using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.API.Body.Repository.Interfaces;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHealth.API.Body.Repository
{
    public class BodyRepository : IBodyRepository
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public BodyRepository(IConfiguration configuration, CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task<List<WeightEnvelope>> ReadAllWeightRecords()
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Weight'");

                List<WeightEnvelope> sleepEnvelopes = new List<WeightEnvelope>();

                FeedIterator<WeightEnvelope> feedIterator = _container.GetItemQueryIterator<WeightEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<WeightEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    sleepEnvelopes.AddRange(queryResponse.Resource);
                }

                return sleepEnvelopes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<WeightEnvelope> ReadWeightRecordByDate(string weightLogDate)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Weight' AND c.Weight.Date = @weightLogDate")
                    .WithParameter("@weightLogDate", weightLogDate);

                List<WeightEnvelope> sleepEnvelopes = new List<WeightEnvelope>();

                FeedIterator<WeightEnvelope> feedIterator = _container.GetItemQueryIterator<WeightEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<WeightEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    sleepEnvelopes.AddRange(queryResponse.Resource);
                }

                return sleepEnvelopes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
