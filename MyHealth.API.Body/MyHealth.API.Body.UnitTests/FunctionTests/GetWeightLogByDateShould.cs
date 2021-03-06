using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Body.Functions;
using MyHealth.API.Body.Services.Interfaces;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.UnitTests.FunctionTests
{
    public class GetWeightLogByDateShould
    {
        private Mock<IBodyService> _mockBodyService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger<GetWeightLogByDate>> _mockLogger;

        private GetWeightLogByDate _func;

        public GetWeightLogByDateShould()
        {
            _mockBodyService = new Mock<IBodyService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger<GetWeightLogByDate>>();

            _func = new GetWeightLogByDate(
                _mockBodyService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Theory]
        [InlineData("2020-12-100")]
        [InlineData("2020-100-12")]
        [InlineData("20201-12-11")]
        public async Task ThrowBadRequestResultWhenWeightLogDateRequestIsInvalid(string invalidDateInput)
        {
            // Arrange
            var weightEnvelope = new mdl.WeightEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyService.Setup(x => x.IsWeightLogDateValid(invalidDateInput)).Returns(false);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, invalidDateInput);

            // Assert
            Assert.Equal(typeof(BadRequestResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(400, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowNotFoundResultWhenWeightEnvelopeResponseIsNull()
        {
            // Arrange
            var weightEnvelope = new mdl.WeightEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyService.Setup(x => x.IsWeightLogDateValid(It.IsAny<string>())).Returns(true);
            _mockBodyService.Setup(x => x.GetWeightRecordByDate(It.IsAny<string>())).Returns(Task.FromResult<mdl.WeightEnvelope>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object,"2019-12-31");

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenWeightRecordIsFound()
        {
            // Arrange
            mdl.WeightEnvelope weightEnvelope = new mdl.WeightEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Sleep",
                Weight = new mdl.Weight
                {
                    Date = "2019-12-31"
                }
            };
            var weightDate = weightEnvelope.Weight.Date;
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyService.Setup(x => x.IsWeightLogDateValid(weightDate)).Returns(true);
            _mockBodyService.Setup(x => x.GetWeightRecordByDate(weightDate)).ReturnsAsync(weightEnvelope);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object,weightDate);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenBodyDbServiceThrowsException()
        {
            // Arrange
            var weightEnvelope = new mdl.WeightEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(weightEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockBodyService.Setup(x => x.IsWeightLogDateValid(It.IsAny<string>())).Returns(true);
            _mockBodyService.Setup(x => x.GetWeightRecordByDate(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, "2019-12-31");

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
