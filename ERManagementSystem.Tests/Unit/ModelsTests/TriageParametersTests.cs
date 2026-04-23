// File: Unit/ModelsTests/TriageParametersTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class TriageParametersTests
    {
        [Fact]
        public void ValidateParameters_AllValuesInRange_DoesNotThrow()
        {
            // Arrange
            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 2,
                Bleeding = 3,
                Injury_Type = 2,
                Pain_Level = 1
            };

            // Act
            var exception = Record.Exception(() => parameters.ValidateParameters());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateParameters_ConsciousnessOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parameters = new Triage_Parameters
            {
                Consciousness = 0,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => parameters.ValidateParameters());

            // Assert
            Assert.Equal("Consciousness", exception.ParamName);
        }

        [Fact]
        public void ValidateParameters_BreathingOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 4,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => parameters.ValidateParameters());

            // Assert
            Assert.Equal("Breathing", exception.ParamName);
        }

        [Fact]
        public void Constructor_InvalidPainLevel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange / Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => new Triage_Parameters(1, 1, 1, 1, 1, 0));

            // Assert
            Assert.Equal("Pain_Level", exception.ParamName);
        }
    }
}