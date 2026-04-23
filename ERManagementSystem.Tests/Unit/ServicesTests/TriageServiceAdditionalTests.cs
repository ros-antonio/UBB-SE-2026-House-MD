// File: Unit/ServicesTests/TriageServiceAdditionalTests.cs
using System;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class TriageServiceAdditionalTests
    {
        [Fact]
        public void CreateTriageParameters_ValidParameters_DelegatesToRepository()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var parameters = new Triage_Parameters
            {
                Triage_ID = 12,
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            service.CreateTriageParameters(parameters);

            // Assert
            triageParametersRepositoryMock.Verify(repository => repository.Add(parameters), Times.Once);
        }

        [Fact]
        public void RequestAvailableNurse_DefaultNurseService_ReturnsFirstAvailableNurseId()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var result = service.RequestAvailableNurse();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CreateTriage_TriageRepositoryThrows_PropagatesExceptionAndDoesNotPersistParameters()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            triageRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Triage>()))
                .Throws(new InvalidOperationException("Insert triage failed."));

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.CreateTriage(25, parameters));

            // Assert
            Assert.Equal("Insert triage failed.", exception.Message);
            triageParametersRepositoryMock.Verify(repository => repository.Add(It.IsAny<Triage_Parameters>()), Times.Never);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreateTriage_TriageParametersRepositoryThrows_PropagatesExceptionAndDoesNotChangeVisitStatus()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 2,
                Pain_Level = 2
            };

            triageRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Triage>()))
                .Returns(77);

            triageParametersRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Triage_Parameters>()))
                .Throws(new InvalidOperationException("Insert triage parameters failed."));

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.CreateTriage(31, parameters));

            // Assert
            Assert.Equal("Insert triage parameters failed.", exception.Message);
            Assert.Equal(77, parameters.Triage_ID);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}