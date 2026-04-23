// File: Unit/ServicesTests/MockStaffServiceTests.cs
using ERManagementSystem.Core.Services;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class MockStaffServiceTests
    {
        [Theory]
        [InlineData("orthopedics", 102)]
        [InlineData(" Orthopedics ", 102)]
        [InlineData("neurology", 103)]
        [InlineData("pulmonology", 105)]
        [InlineData("emergency medicine", 106)]
        [InlineData("general surgery", 104)]
        [InlineData("general", 104)]
        [InlineData("unknown-specialization", 104)]
        public void RequestDoctor_Specialization_ReturnsExpectedDoctorId(string specialization, int expectedDoctorId)
        {
            // Arrange
            var service = new MockStaffService();

            // Act
            var result = service.RequestDoctor(specialization, null!);

            // Assert
            Assert.Equal(expectedDoctorId, result);
        }

        [Fact]
        public void GetDoctorByID_KnownDoctorId_ReturnsExpectedDoctor()
        {
            // Arrange
            var service = new MockStaffService();

            // Act
            var result = service.GetDoctorByID(105);

            // Assert
            var expectedDoctor = new ERManagementSystem.Core.Models.Doctor
            {
                DoctorID = 105,
                Name = "Dr. Taylor",
                Specialty = "Pulmonology"
            };

            Assert.Equivalent(expectedDoctor, result, strict: true);
        }

        [Fact]
        public void GetDoctorByID_UnknownDoctorId_ReturnsUnknownDoctor()
        {
            // Arrange
            var service = new MockStaffService();

            // Act
            var result = service.GetDoctorByID(999);

            // Assert
            var expectedDoctor = new ERManagementSystem.Core.Models.Doctor
            {
                DoctorID = 0,
                Name = "Unknown",
                Specialty = "Unknown"
            };

            Assert.Equivalent(expectedDoctor, result, strict: true);
        }
    }
}