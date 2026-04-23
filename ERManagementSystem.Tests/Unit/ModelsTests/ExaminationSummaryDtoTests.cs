// File: Unit/ModelsTests/ExaminationSummaryDtoTests.cs
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class ExaminationSummaryDtoTests
    {
        [Fact]
        public void SeverityScore_ValidValues_ReturnsExpectedFormattedScore()
        {
            // Arrange
            var dto = new ExaminationSummaryDTO
            {
                Consciousness = 3,
                Breathing = 2,
                Bleeding = 1,
                InjuryType = 2,
                PainLevel = 3
            };

            // Act
            var result = dto.SeverityScore;

            // Assert
            Assert.Equal("11 / 15", result);
        }
    }
}