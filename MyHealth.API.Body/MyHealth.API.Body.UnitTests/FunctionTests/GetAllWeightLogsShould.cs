using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Body.Functions;
using MyHealth.API.Body.Services;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.UnitTests.FunctionTests
{
    public class GetAllWeightLogsShould
    {
        private Mock<IBodyDbService> _mockBodyDbService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetAllWeightLogs _func;

        public GetAllWeightLogsShould()
        {
            _mockBodyDbService = new Mock<IBodyDbService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetAllWeightLogs(
                _mockBodyDbService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenWeightRecordsAreFound()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();
            var fixture = new Fixture();
            mdl.WeightEnvelope weightEnvelope = fixture.Create<mdl.WeightEnvelope>();
            weightEnvelopes.Add(weightEnvelope);
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelopes));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyDbService.Setup(x => x.GetWeightRecords()).ReturnsAsync(weightEnvelopes);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNoWeightRecordsAreFound()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelopes));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyDbService.Setup(x => x.GetWeightRecords()).ReturnsAsync(weightEnvelopes);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowBadRequestResultWhenWeightEnvelopesAreNull()
        {
            // Arrange
            MemoryStream memoryStream = new MemoryStream();
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);
            _mockBodyDbService.Setup(x => x.GetWeightRecords()).Returns(Task.FromResult<List<mdl.WeightEnvelope>>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenBodyDbServiceThrowsException()
        {
            // Arrange
            List<mdl.WeightEnvelope> weightEnvelopes = new List<mdl.WeightEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelopes));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyDbService.Setup(x => x.GetWeightRecords()).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
