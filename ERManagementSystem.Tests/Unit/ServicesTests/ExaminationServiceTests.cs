// File: Unit/ServicesTests/ExaminationServiceTests.cs
using System;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class ExaminationServiceTests
    {
        [Fact]
        public void RequestDoctor_TriageMissing_ThrowsException()
        {
            // Arrange
            var examRepositoryMock = new Mock<IExaminationRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(10))
                .Returns((Triage?)null);

            var service = new ExaminationService(
                examRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                triageRepositoryMock.Object,
                new MockStaffService(),
                stateManagementServiceMock.Object,
                triageParamsRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<Exception>(() => service.RequestDoctor(10));

            // Assert
            Assert.Equal("Triage record not found for visit 10", exception.Message);
            stateManagementServiceMock.Verify(
                service => service.ChangeVisitStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void RequestDoctor_TriageParametersMissing_ThrowsException()
        {
            // Arrange
            var examRepositoryMock = new Mock<IExaminationRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();

            var triage = new Triage
            {
                Triage_ID = 5,
                Visit_ID = 10,
                Specialization = "Orthopedics"
            };

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(10))
                .Returns(triage);

            triageParamsRepositoryMock
                .Setup(repository => repository.GetByTriageId(5))
                .Returns((Triage_Parameters?)null);

            var service = new ExaminationService(
                examRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                triageRepositoryMock.Object,
                new MockStaffService(),
                stateManagementServiceMock.Object,
                triageParamsRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<Exception>(() => service.RequestDoctor(10));

            // Assert
            Assert.Equal("Triage parameters not found for triage 5", exception.Message);
            stateManagementServiceMock.Verify(
                service => service.ChangeVisitStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void RequestDoctor_ValidData_ReturnsAssignedDoctorAndChangesStatus()
        {
            // Arrange
            var examRepositoryMock = new Mock<IExaminationRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();

            var triage = new Triage
            {
                Triage_ID = 7,
                Visit_ID = 20,
                Specialization = "Orthopedics"
            };

            var parameters = new Triage_Parameters
            {
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                Injury_Type = 2,
                Pain_Level = 2
            };

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(20))
                .Returns(triage);

            triageParamsRepositoryMock
                .Setup(repository => repository.GetByTriageId(7))
                .Returns(parameters);

            var service = new ExaminationService(
                examRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                triageRepositoryMock.Object,
                new MockStaffService(),
                stateManagementServiceMock.Object,
                triageParamsRepositoryMock.Object);

            // Act
            var result = service.RequestDoctor(20);

            // Assert
            Assert.Equal(102, result);

            stateManagementServiceMock.Verify(
                stateService => stateService.ChangeVisitStatus(20, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR),
                Times.Once);
        }

        [Fact]
        public void SaveExamination_ValidExam_AddsExamAndChangesStatus()
        {
            // Arrange
            var examRepositoryMock = new Mock<IExaminationRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();

            var exam = new Examination
            {
                Exam_ID = 1,
                Visit_ID = 30,
                Doctor_ID = 104,
                Room_ID = 2,
                Exam_Time = new DateTime(2026, 4, 23, 10, 0, 0),
                Notes = "Stable."
            };

            var service = new ExaminationService(
                examRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                triageRepositoryMock.Object,
                new MockStaffService(),
                stateManagementServiceMock.Object,
                triageParamsRepositoryMock.Object);

            // Act
            service.SaveExamination(exam);

            // Assert
            examRepositoryMock.Verify(repository => repository.Add(exam), Times.Once);

            stateManagementServiceMock.Verify(
                stateService => stateService.ChangeVisitStatus(30, ER_Visit.VisitStatus.IN_EXAMINATION),
                Times.Once);
        }
    }
}