using MyHealth.API.Body.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MyHealth.API.Body.UnitTests.ValidatorTests
{
    public class DateValidatorShould
    {
        private DateValidator _sut;

        public DateValidatorShould()
        {
            _sut = new DateValidator();
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
