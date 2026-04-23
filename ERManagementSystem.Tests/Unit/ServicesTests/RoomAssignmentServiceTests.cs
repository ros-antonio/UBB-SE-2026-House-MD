// File: Unit/ServicesTests/RoomAssignmentServiceTests.cs
using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using ERManagementSystem.Core.Helpers;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class RoomAssignmentServiceTests
    {
        [Fact]
        public void GetWaitingVisitsWithTriage_MixedStatuses_ReturnsOnlyWaitingForRoomOrdered()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var queue = new List<(ER_Visit visit, Triage triage)>
            {
                (
                    new ER_Visit
                    {
                        Visit_ID = 1,
                        Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                        Arrival_date_time = new DateTime(2026, 4, 23, 9, 30, 0)
                    },
                    new Triage { Triage_ID = 1, Triage_Level = 3 }
                ),
                (
                    new ER_Visit
                    {
                        Visit_ID = 2,
                        Status = ER_Visit.VisitStatus.IN_ROOM,
                        Arrival_date_time = new DateTime(2026, 4, 23, 9, 0, 0)
                    },
                    new Triage { Triage_ID = 2, Triage_Level = 1 }
                ),
                (
                    new ER_Visit
                    {
                        Visit_ID = 3,
                        Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                        Arrival_date_time = new DateTime(2026, 4, 23, 8, 45, 0)
                    },
                    new Triage { Triage_ID = 3, Triage_Level = 2 }
                )
            };

            erVisitRepositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(queue);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetWaitingVisitsWithTriage();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[0].visit.Visit_ID);
            Assert.Equal(1, result[1].visit.Visit_ID);
        }

        [Fact]
        public void FindAvailableRoom_MatchingRoomExists_ReturnsMatchingRoom()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var rooms = new List<ER_Room>
            {
                new ER_Room { Room_ID = 1, Room_Type = ER_Room.RoomType.GeneralRoom, Availability_Status = ER_Room.RoomStatus.Available },
                new ER_Room { Room_ID = 2, Room_Type = ER_Room.RoomType.OperatingRoom, Availability_Status = ER_Room.RoomStatus.Available }
            };

            roomRepositoryMock
                .Setup(repository => repository.GetAvailableRooms())
                .Returns(rooms);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.FindAvailableRoom(ER_Room.RoomType.OperatingRoom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Room_ID);
        }

        [Fact]
        public void AssignRoomToVisit_RoomMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            roomRepositoryMock
                .Setup(repository => repository.GetById(10))
                .Returns((ER_Room?)null);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.AssignRoomToVisit(100, 10));

            // Assert
            Assert.Equal("Room 10 was not found.", exception.Message);
        }

        [Fact]
        public void AssignRoomToVisit_RoomNotAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 10,
                Availability_Status = ER_Room.RoomStatus.Occupied
            };

            roomRepositoryMock
                .Setup(repository => repository.GetById(10))
                .Returns(room);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.AssignRoomToVisit(100, 10));

            // Assert
            Assert.Equal("Room 10 is not available (current: 'occupied').", exception.Message);
        }

        [Fact]
        public void AssignRoomToVisit_VisitMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 10,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            roomRepositoryMock
                .Setup(repository => repository.GetById(10))
                .Returns(room);

            erVisitRepositoryMock
                .Setup(repository => repository.GetByVisitId(100))
                .Returns((ER_Visit?)null);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.AssignRoomToVisit(100, 10));

            // Assert
            Assert.Equal("Visit 100 was not found.", exception.Message);
        }

        [Fact]
        public void AssignRoomToVisit_VisitNotWaitingForRoom_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 10,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            var visit = new ER_Visit
            {
                Visit_ID = 100,
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            roomRepositoryMock
                .Setup(repository => repository.GetById(10))
                .Returns(room);

            erVisitRepositoryMock
                .Setup(repository => repository.GetByVisitId(100))
                .Returns(visit);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.AssignRoomToVisit(100, 10));

            // Assert
            Assert.Equal("Visit 100 is not in WAITING_FOR_ROOM (current: 'TRIAGED').", exception.Message);
        }

        [Fact]
        public void AssignRoomToVisit_ValidData_UpdatesRoomSetsVisitAndChangesState()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 10,
                Availability_Status = ER_Room.RoomStatus.Available,
                Room_Type = ER_Room.RoomType.GeneralRoom
            };

            var visit = new ER_Visit
            {
                Visit_ID = 100,
                Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM
            };

            roomRepositoryMock
                .Setup(repository => repository.GetById(10))
                .Returns(room);

            erVisitRepositoryMock
                .Setup(repository => repository.GetByVisitId(100))
                .Returns(visit);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            service.AssignRoomToVisit(100, 10);

            // Assert
            roomRepositoryMock.Verify(repository => repository.UpdateAvailabilityStatus(10, ER_Room.RoomStatus.Occupied), Times.Once);
            roomRepositoryMock.Verify(repository => repository.SetCurrentVisit(10, 100), Times.Once);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(100, ER_Visit.VisitStatus.IN_ROOM), Times.Once);
            Assert.Equal(ER_Room.RoomStatus.Occupied, room.Availability_Status);
        }

        [Fact]
        public void UpdateRoomAvailability_RoomMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            roomRepositoryMock
                .Setup(repository => repository.GetById(3))
                .Returns((ER_Room?)null);

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.UpdateRoomAvailability(3, ER_Room.RoomStatus.Occupied));

            // Assert
            Assert.Equal("Room 3 was not found.", exception.Message);
        }

        [Fact]
        public void AutoAssignRoom_NoWaitingVisits_ReturnsFalse()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            erVisitRepositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(new List<(ER_Visit visit, Triage triage)>());

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.AutoAssignRoom();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AutoAssignRoom_NoMatchingAvailableRoom_ReturnsFalse()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var topVisit = new ER_Visit
            {
                Visit_ID = 100,
                Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                Arrival_date_time = new DateTime(2026, 4, 23, 8, 0, 0)
            };

            var topTriage = new Triage
            {
                Triage_ID = 10,
                Visit_ID = 100,
                Triage_Level = 2,
                Specialization = "Orthopedics"
            };

            erVisitRepositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(new List<(ER_Visit visit, Triage triage)> { (topVisit, topTriage) });

            triageParamsRepositoryMock
                .Setup(repository => repository.GetByTriageId(10))
                .Returns(new Triage_Parameters
                {
                    Consciousness = 1,
                    Breathing = 1,
                    Bleeding = 1,
                    Injury_Type = 2,
                    Pain_Level = 1
                });

            roomRepositoryMock
                .Setup(repository => repository.GetAvailableRooms())
                .Returns(new List<ER_Room>());

            var service = new RoomAssignmentService(
                roomRepositoryMock.Object,
                erVisitRepositoryMock.Object,
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.AutoAssignRoom();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AutoAssignRoom_MatchingRoomExists_AssignsRoomAndReturnsTrue()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var erVisitRepositoryMock = new Mock<IERVisitRepository>();
            var stateServiceMock = new Mock<IStateManagementService>();
            var triageParamsRepositoryMock = new Mock<ITriageParametersRepository>();
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
                Specialization = "Orthopedics"
            };

            var room = new ER_Room
            {
                Room_ID = 5,
                Room_Type = ER_Room.RoomType.OrthopedicRoom,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            erVisitRepositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(new List<(ER_Visit visit, Triage triage)> { (visit, triage) });

            triageParamsRepositoryMock
                .Setup(repository => repository.GetByTriageId(10))
                .Returns(new Triage_Parameters
                {
                    Consciousness = 1,
                    Breathing = 1,
                    Bleeding = 1,
                    Injury_Type = 2,
                    Pain_Level = 1
                });

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
                stateServiceMock.Object,
                triageParamsRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.AutoAssignRoom();

            // Assert
            Assert.True(result);
            roomRepositoryMock.Verify(repository => repository.SetCurrentVisit(5, 100), Times.Once);
            stateServiceMock.Verify(service => service.ChangeVisitStatus(100, ER_Visit.VisitStatus.IN_ROOM), Times.Once);
        }
    }
}