// File: Unit/ModelsTests/TransferLogTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class TransferLogTests
    {
        [Fact]
        public void Status_ValidStatus_StoresValue()
        {
            // Arrange
            var log = new Transfer_Log();

            // Act
            log.Status = "SUCCESS";

            // Assert
            Assert.Equal("SUCCESS", log.Status);
        }

        [Fact]
        public void Status_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var log = new Transfer_Log();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => log.Status = "DONE");

            // Assert
            Assert.Contains("Invalid status 'DONE'.", exception.Message);
        }

        [Fact]
        public void Validate_ValidLog_DoesNotThrow()
        {
            // Arrange
            var log = new Transfer_Log
            {
                Transfer_ID = 1,
                Visit_ID = 10,
                Transfer_Time = new DateTime(2026, 4, 23, 15, 0, 0),
                Target_System = "Patient Management",
                Status = "RETRYING"
            };

            // Act
            var exception = Record.Exception(() => log.Validate());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_EmptyTargetSystem_ThrowsArgumentException()
        {
            // Arrange
            var log = new Transfer_Log
            {
                Visit_ID = 10,
                Target_System = "",
                Status = "SUCCESS"
            };

            // Act
            var exception = Assert.Throws<ArgumentException>(() => log.Validate());

            // Assert
            Assert.Equal("Target_System must not be empty.", exception.Message);
        }

        [Fact]
        public void Validate_InvalidVisitId_ThrowsArgumentException()
        {
            // Arrange
            var log = new Transfer_Log
            {
                Visit_ID = 0,
                Target_System = "Patient Management",
                Status = "FAILED"
            };

            // Act
            var exception = Assert.Throws<ArgumentException>(() => log.Validate());

            // Assert
            Assert.Equal("Visit_ID must be a valid positive integer.", exception.Message);
        }
    }
}