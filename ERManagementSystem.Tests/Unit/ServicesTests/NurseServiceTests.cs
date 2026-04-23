// File: Unit/ServicesTests/NurseServiceTests.cs
using ERManagementSystem.Core.Services;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class NurseServiceTests
    {
        [Fact]
        public void RequestAvailableNurse_DefaultMockData_ReturnsFirstAvailableNurseId()
        {
            // Arrange
            var service = new NurseService();

            // Act
            var result = service.RequestAvailableNurse();

            // Assert
            Assert.Equal(2, result);
        }
    }
}