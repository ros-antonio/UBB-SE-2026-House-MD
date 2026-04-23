// File: Unit/ServicesTests/TriageServiceTests.cs
using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class TriageServiceTests
    {
        [Fact]
        public void RequestAvailableNurse_DefaultNurseService_ReturnsAvailableNurseId()
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
        public void CreateTriage_InvalidParameters_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var invalidParameters = new Triage_Parameters
            {
                Consciousness = 0,
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
            Assert.Throws<ArgumentOutOfRangeException>(() => service.CreateTriage(5, invalidParameters));

            // Assert
            triageRepositoryMock.Verify(repository => repository.Add(It.IsAny<Triage>()), Times.Never);
            triageParametersRepositoryMock.Verify(repository => repository.Add(It.IsAny<Triage_Parameters>()), Times.Never);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreateTriage_ValidParameters_AddsTriageAddsParametersAndChangesStatus()
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
                .Returns(99);

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var result = service.CreateTriage(5, parameters);

            // Assert
            Assert.Equal(99, result.Triage_ID);
            Assert.Equal(5, result.Visit_ID);
            Assert.Equal(4, result.Triage_Level);
            Assert.Equal("Orthopedics", result.Specialization);
            Assert.Equal(2, result.Nurse_ID);

            Assert.Equal(99, parameters.Triage_ID);

            triageRepositoryMock.Verify(repository => repository.Add(It.IsAny<Triage>()), Times.Once);
            triageParametersRepositoryMock.Verify(repository => repository.Add(parameters), Times.Once);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(5, ER_Visit.VisitStatus.TRIAGED), Times.Once);
        }

        [Fact]
        public void GetByVisitId_RepositoryReturnsTriage_ReturnsSameObject()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var triage = new Triage { Visit_ID = 5, Triage_ID = 50 };

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(5))
                .Returns(triage);

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var result = service.GetByVisitId(5);

            // Assert
            Assert.Same(triage, result);
        }

        [Fact]
        public void GetVisitsForTriage_RegisteredAndTriagedVisits_ReturnsMergedAndOrderedByArrival()
        {
            // Arrange
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();

            var registeredVisits = new List<ER_Visit>
            {
                new ER_Visit
                {
                    Visit_ID = 2,
                    Status = ER_Visit.VisitStatus.REGISTERED,
                    Arrival_date_time = new DateTime(2026, 4, 23, 10, 0, 0)
                }
            };

            var triagedVisits = new List<ER_Visit>
            {
                new ER_Visit
                {
                    Visit_ID = 1,
                    Status = ER_Visit.VisitStatus.TRIAGED,
                    Arrival_date_time = new DateTime(2026, 4, 23, 9, 0, 0)
                }
            };

            stateServiceMock
                .Setup(service => service.GetByStatus(ER_Visit.VisitStatus.REGISTERED))
                .Returns(registeredVisits);

            stateServiceMock
                .Setup(service => service.GetByStatus(ER_Visit.VisitStatus.TRIAGED))
                .Returns(triagedVisits);

            var service = new TriageService(
                triageRepositoryMock.Object,
                triageParametersRepositoryMock.Object,
                new NurseService(),
                stateServiceMock.Object);

            // Act
            var result = service.GetVisitsForTriage();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Visit_ID);
            Assert.Equal(2, result[1].Visit_ID);
        }

        [Fact]
        public void MoveVisitToQueue_ValidVisit_ChangesStatusToWaitingForRoom()
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
            service.MoveVisitToQueue(10);

            // Assert
            stateServiceMock.Verify(
                service => service.ChangeVisitStatus(10, ER_Visit.VisitStatus.WAITING_FOR_ROOM),
                Times.Once);
        }

        [Fact]
        public void CloseVisit_DelegatesToStateService()
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
            service.CloseVisit(12);

            // Assert
            stateServiceMock.Verify(service => service.CloseVisit(12), Times.Once);
        }

        [Fact]
        public void CalculateTriageLevel_CriticalCondition_ReturnsLevelOne()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 3,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.CalculateTriageLevel(parameters);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void CalculateTriageLevel_SeverityScoreTwentyOrMore_ReturnsLevelTwo()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 2,
                Breathing = 2,
                Bleeding = 2,
                Injury_Type = 2,
                Pain_Level = 2
            };

            // Act
            var result = service.CalculateTriageLevel(parameters);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void CalculateTriageLevel_SeverityScoreBetweenSixteenAndNineteen_ReturnsLevelThree()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 2,
                Breathing = 2,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.CalculateTriageLevel(parameters);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void CalculateTriageLevel_SeverityScoreBetweenTwelveAndFifteen_ReturnsLevelFour()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 2,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.CalculateTriageLevel(parameters);

            // Assert
            Assert.Equal(4, result);
        }

        [Fact]
        public void CalculateTriageLevel_SeverityScoreElevenOrLess_ReturnsLevelFive()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.CalculateTriageLevel(parameters);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void DetermineSpecialization_BleedingCritical_ReturnsGeneralSurgery()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 3,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.DetermineSpecialization(parameters);

            // Assert
            Assert.Equal("General Surgery", result);
        }

        [Fact]
        public void DetermineSpecialization_InjuryTypeTwo_ReturnsOrthopedics()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 2,
                Pain_Level = 1
            };

            // Act
            var result = service.DetermineSpecialization(parameters);

            // Assert
            Assert.Equal("Orthopedics", result);
        }

        [Fact]
        public void DetermineSpecialization_BreathingTwo_ReturnsPulmonology()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 2,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.DetermineSpecialization(parameters);

            // Assert
            Assert.Equal("Pulmonology", result);
        }

        [Fact]
        public void DetermineSpecialization_ConsciousnessTwo_ReturnsNeurology()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 2,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.DetermineSpecialization(parameters);

            // Assert
            Assert.Equal("Neurology", result);
        }

        [Fact]
        public void DetermineSpecialization_NoRuleMatched_ReturnsEmergencyMedicine()
        {
            // Arrange
            var service = CreateServiceForPureLogic();

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 1,
                Pain_Level = 1
            };

            // Act
            var result = service.DetermineSpecialization(parameters);

            // Assert
            Assert.Equal("Emergency Medicine", result);
        }

        private static TriageService CreateServiceForPureLogic()
        {
            return new TriageService(
                new Mock<ITriageRepository>().Object,
                new Mock<ITriageParametersRepository>().Object,
                new NurseService(),
                new Mock<IStateManagementService>().Object);
        }
    }
}