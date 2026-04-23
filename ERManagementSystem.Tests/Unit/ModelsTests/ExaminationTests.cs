// File: Unit/ModelsTests/ExaminationTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class ExaminationTests
    {
        [Fact]
        public void Validate_ValidExamination_ReturnsTrueAndNoErrors()
        {
            // Arrange
            var exam = CreateValidExamination();

            // Act
            var result = exam.Validate(out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_InvalidFields_ReturnsFalseAndContainsAllRelevantErrors()
        {
            // Arrange
            var exam = new Examination
            {
                Exam_ID = 0,
                Visit_ID = 0,
                Doctor_ID = 0,
                Room_ID = 0,
                Notes = "",
                Exam_Time = default
            };

            // Act
            var result = exam.Validate(out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Exam_ID must be a positive integer.", errors);
            Assert.Contains("Visit_ID is required and must be valid.", errors);
            Assert.Contains("Doctor_ID is required.", errors);
            Assert.Contains("Room_ID is required.", errors);
            Assert.Contains("Notes must not be empty", errors);
            Assert.Contains("Examination date and time is required", errors);
        }

        [Fact]
        public void ToString_ValidExamination_ReturnsExpectedFormat()
        {
            // Arrange
            var examTime = new DateTime(2026, 4, 23, 14, 30, 0);
            var exam = new Examination(11, 22, 33, examTime, 44, "Patient stable");

            // Act
            var result = exam.ToString();

            // Assert
            Assert.Equal(
                "[Examination 11] [Visit 22] Doctor: 33  | Examination Time: 2026-04-23 14:30 | Room: 44 | Notes: Patient stable",
                result);
        }

        private static Examination CreateValidExamination()
        {
            return new Examination
            {
                Exam_ID = 1,
                Visit_ID = 2,
                Doctor_ID = 3,
                Room_ID = 4,
                Notes = "Patient stable",
                Exam_Time = new DateTime(2026, 4, 23, 14, 30, 0)
            };
        }
    }
}