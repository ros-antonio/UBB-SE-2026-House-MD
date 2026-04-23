// File: Unit/ModelsTests/PatientTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class PatientTests
    {
        [Fact]
        public void Validate_ValidPatient_ReturnsTrueAndNoErrors()
        {
            // Arrange
            var patient = CreateValidPatient();

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MissingPatientId_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Patient_ID = "";

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Patient ID (CNP) is required.", errors);
        }

        [Fact]
        public void Validate_PatientIdTooLong_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Patient_ID = new string('1', 21);

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Patient ID (CNP) must not exceed 20 characters.", errors);
        }

        [Fact]
        public void Validate_FirstNameTooLong_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.First_Name = new string('A', 51);

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("First name must not exceed 50 characters.", errors);
        }

        [Fact]
        public void Validate_LastNameMissing_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Last_Name = " ";

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Last name is required.", errors);
        }

        [Fact]
        public void Validate_DateOfBirthDefault_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Date_of_Birth = default;

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Date of birth is required.", errors);
        }

        [Fact]
        public void Validate_DateOfBirthToday_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Date_of_Birth = DateTime.Today;

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Date of birth must be in the past.", errors);
        }

        [Fact]
        public void Validate_InvalidGender_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Gender = "Other";

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Gender must be one of: Male, Female.", errors);
        }

        [Fact]
        public void Validate_PhoneTooLong_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Phone = new string('1', 21);

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Phone number must not exceed 20 characters.", errors);
        }

        [Fact]
        public void Validate_EmergencyContactTooLong_ReturnsFalseAndContainsError()
        {
            // Arrange
            var patient = CreateValidPatient();
            patient.Emergency_Contact = new string('A', 101);

            // Act
            var result = patient.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Emergency contact must not exceed 100 characters.", errors);
        }

        private static Patient CreateValidPatient()
        {
            return new Patient
            {
                Patient_ID = "1234567890123",
                First_Name = "Ana",
                Last_Name = "Pop",
                Date_of_Birth = new DateTime(2000, 1, 1),
                Gender = "Female",
                Phone = "0712345678",
                Emergency_Contact = "Maria Pop"
            };
        }
    }
}