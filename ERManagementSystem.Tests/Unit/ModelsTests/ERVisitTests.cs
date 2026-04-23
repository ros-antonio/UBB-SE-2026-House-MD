// File: Unit/ModelsTests/ERVisitTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class ERVisitTests
    {
        [Fact]
        public void Validate_ValidVisit_ReturnsTrueAndNoErrors()
        {
            // Arrange
            var visit = CreateValidVisit();

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MissingPatientId_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Patient_ID = "";

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Patient ID is required.", errors);
        }

        [Fact]
        public void Validate_DefaultArrivalDateTime_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Arrival_date_time = default;

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Arrival date and time is required.", errors);
        }

        [Fact]
        public void Validate_MissingChiefComplaint_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Chief_Complaint = " ";

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Chief complaint is required.", errors);
        }

        [Fact]
        public void Validate_ChiefComplaintTooLong_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Chief_Complaint = new string('A', 256);

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Chief complaint must not exceed 255 characters.", errors);
        }

        [Fact]
        public void Validate_MissingStatus_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Status = "";

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Status is required.", errors);
        }

        [Fact]
        public void Validate_InvalidStatus_ReturnsFalseAndContainsError()
        {
            // Arrange
            var visit = CreateValidVisit();
            visit.Status = "INVALID_STATUS";

            // Act
            var result = visit.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Invalid status 'INVALID_STATUS'. Must be one of:", errors[0]);
        }

        private static ER_Visit CreateValidVisit()
        {
            return new ER_Visit
            {
                Visit_ID = 1,
                Patient_ID = "1234567890123",
                Arrival_date_time = new DateTime(2026, 4, 23, 10, 0, 0),
                Chief_Complaint = "Chest pain",
                Status = ER_Visit.VisitStatus.REGISTERED
            };
        }
    }
}