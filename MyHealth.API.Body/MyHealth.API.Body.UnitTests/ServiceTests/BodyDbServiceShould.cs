using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.API.Body.Services;
using MyHealth.API.Body.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.UnitTests.ServiceTests
{
    public class BodyDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private BodyDbService _sut;

        public BodyDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new BodyDbService(
                _mockConfiguration.Object,
                _mockCosmosClient.Object);
        }

        [Fact]
        public async Task GetAllWeightLogsRecordsSuccessfully()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();
            var fixture = new Fixture();
            mdl.WeightEnvelope weightEnvelope = fixture.Create<mdl.WeightEnvelope>();
            weightEnvelopes.Add(weightEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(weightEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { weightEnvelopes.Count });

            // Act
            var response = await _sut.GetWeightRecords();

            // Assert
            Assert.Equal(weightEnvelopes.Count, response.Count);
        }

        [Fact]
        public async Task GetAllWeightLogRecordsSuccessfully_NoResultsReturned()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();

            _mockContainer.SetupItemQueryIteratorMock(weightEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { weightEnvelopes.Count });

            // Act
            var response = await _sut.GetWeightRecords();

            // Assert
            Assert.Equal(weightEnvelopes.Count, response.Count);

        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetWeightRecordsIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.WeightEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetWeightRecords();

            // Assert
            await responseAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetWeightLogByDateSuccessfully()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();
            mdl.WeightEnvelope weightEnvelope = new mdl.WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Sleep",
                Weight = new mdl.Weight
                {
                    Date = "2019-12-31"
                }
            };
            weightEnvelopes.Add(weightEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(weightEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { weightEnvelopes.Count });

            var weightDate = weightEnvelope.Weight.Date;

            // Act
            var response = await _sut.GetWeightRecordByDate(weightDate);

            // Assert
            Assert.Equal(weightDate, response.Weight.Date);
        }

        [Fact]
        public async Task GetWeightLogByDate_NoResultsReturned()
        {
            // Arrange
            var emptyWeightList = new List<mdl.WeightEnvelope>();

            var getWeightLogByDate = _mockContainer.SetupItemQueryIteratorMock(emptyWeightList);
            getWeightLogByDate.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetWeightRecordByDate("2019-12-31");

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetWeightLogByDateIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.WeightEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetWeightRecordByDate("2019-12-31");

            // Assert
            await responseAction.Should().ThrowAsync<Exception>();
        }
    }
}
