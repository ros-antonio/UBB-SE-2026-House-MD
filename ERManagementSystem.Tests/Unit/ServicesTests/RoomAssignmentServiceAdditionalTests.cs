// File: Unit/ServicesTests/RoomAssignmentServiceAdditionalTests.cs
using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class RoomAssignmentServiceAdditionalTests
    {
        [Fact]
        public void GetAvailableRooms_RepositoryReturnsRooms_ReturnsSameList()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var expectedRooms = new List<ER_Room>
            {
                new ER_Room { Room_ID = 1, Room_Type = ER_Room.RoomType.GeneralRoom }
            };

            roomRepositoryMock
                .Setup(repository => repository.GetAvailableRooms())
                .Returns(expectedRooms);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateManagementServiceMock.Object,
                triageParametersRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetAvailableRooms();

            // Assert
            Assert.Same(expectedRooms, result);
        }

        [Fact]
        public void GetPatientById_RepositoryReturnsPatient_ReturnsSamePatient()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var patient = new Patient
            {
                Patient_ID = "1234567890123",
                First_Name = "Ana",
                Last_Name = "Pop"
            };

            patientRepositoryMock
                .Setup(repository => repository.GetById("1234567890123"))
                .Returns(patient);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateManagementServiceMock.Object,
                triageParametersRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetPatientById("1234567890123");

            // Assert
            Assert.Same(patient, result);
        }

        [Fact]
        public void GetTriageByVisitId_RepositoryReturnsTriage_ReturnsSameTriage()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var triage = new Triage
            {
                Triage_ID = 18,
                Visit_ID = 50,
                Triage_Level = 3
            };

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(50))
                .Returns(triage);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateManagementServiceMock.Object,
                triageParametersRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetTriageByVisitId(50);

            // Assert
            Assert.Same(triage, result);
        }

        [Fact]
        public void UpdateRoomAvailability_ValidTransition_UpdatesRoomAndRepository()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 6,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            roomRepositoryMock
                .Setup(repository => repository.GetById(6))
                .Returns(room);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateManagementServiceMock.Object,
                triageParametersRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            service.UpdateRoomAvailability(6, ER_Room.RoomStatus.Occupied);

            // Assert
            Assert.Equal(ER_Room.RoomStatus.Occupied, room.Availability_Status);

            roomRepositoryMock.Verify(
                repository => repository.UpdateAvailabilityStatus(6, ER_Room.RoomStatus.Occupied),
                Times.Once);
        }

        [Fact]
        public void AutoAssignRoom_MissingTriageParameters_UsesSafeDefaultsAndAssignsRoom()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateManagementServiceMock = new Mock<IStateManagementService>();
            var triageParametersRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 100,
                Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                Arrival_date_time = new DateTime(2026, 4, 23, 8, 0, 0)
            };

            var triage = new Triage
            {
                Triage_ID = 10,
                Visit_ID = 100,
                Triage_Level = 2,
                Specialization = "Emergency Medicine"
            };

            var room = new ER_Room
            {
                Room_ID = 5,
                Room_Type = ER_Room.RoomType.GeneralRoom,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            erVisitRepositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(new List<(ER_Visit visit, Triage triage)> { (visit, triage) });

            triageParametersRepositoryMock
                .Setup(repository => repository.GetByTriageId(10))
                .Returns((Triage_Parameters?)null);

            roomRepositoryMock
                .Setup(repository => repository.GetAvailableRooms())
                .Returns(new List<ER_Room> { room });

            roomRepositoryMock
                .Setup(repository => repository.GetById(5))
                .Returns(room);

            erVisitRepositoryMock
                .Setup(repository => repository.GetByVisitId(100))
                .Returns(visit);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateManagementServiceMock.Object,
                triageParametersRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.AutoAssignRoom();

            // Assert
            Assert.True(result);
            roomRepositoryMock.Verify(repository => repository.SetCurrentVisit(5, 100), Times.Once);
            stateManagementServiceMock.Verify(
                service => service.ChangeVisitStatus(100, ER_Visit.VisitStatus.IN_ROOM),
                Times.Once);
        }
    }
}