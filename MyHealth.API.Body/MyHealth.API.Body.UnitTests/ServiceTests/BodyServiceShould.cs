using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using MyHealth.API.Body.Repository.Interfaces;
using MyHealth.API.Body.Services;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Body.UnitTests.ServiceTests
{
    public class BodyServiceShould
    {
        private Mock<IBodyRepository> _mockBodyRepository;

        private BodyService _sut;

        public BodyServiceShould()
        {
            _mockBodyRepository = new Mock<IBodyRepository>();

            _sut = new BodyService(_mockBodyRepository.Object);
        }

        [Fact]
        public async Task ThrowExceptionWhenReadWeightRecordByDateFails()
        {
            // Arrange
            _mockBodyRepository.Setup(repo => repo.ReadWeightRecordByDate(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> bodyServiceAction = async () => await _sut.GetWeightRecordByDate("31/10/2021");

            // Assert
            await bodyServiceAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ThrowExceptionWhenReadAllWeightRecordsFails()
        {
            // Arrange
            _mockBodyRepository.Setup(repo => repo.ReadAllWeightRecords()).ThrowsAsync(new Exception());

            // Act
            Func<Task> bodyServiceAction = async () => await _sut.GetWeightRecords();

            // Assert
            await bodyServiceAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ReturnWeightEnvelopeWhenGetWeightRecordByDateIsCalled()
        {
            // Arrange
            var fixture = new Fixture();
            var weightEnvelope = fixture.Create<mdl.WeightEnvelope>();

            _mockBodyRepository.Setup(repo => repo.ReadWeightRecordByDate(It.IsAny<string>())).ReturnsAsync(weightEnvelope);

            // Act
            var response = await _sut.GetWeightRecordByDate(weightEnvelope.Weight.Date);

            // Assert
            using (new AssertionScope())
            {
                response.Weight.Date = weightEnvelope.Weight.Date;
                response.DocumentType = "Weight";
            }
        }

        [Fact]
        public void ReturnFalseIfWeightLogDateIsNotInValidFormat()
        {
            // Arrange
            string testSleepDate = "2021-12-100";

            // Act
            var response = _sut.IsWeightLogDateValid(testSleepDate);

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void ReturnTrueIfWeightLogDateIsInValidFormat()
        {
            // Arrange
            string testSleepDate = "2020-12-31";

            // Act
            var response = _sut.IsWeightLogDateValid(testSleepDate);

            // Assert
            Assert.True(response);
        }
    }
}
