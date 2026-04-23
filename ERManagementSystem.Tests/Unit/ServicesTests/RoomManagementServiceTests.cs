// File: Unit/ServicesTests/RoomManagementServiceTests.cs
using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class RoomManagementServiceTests
    {
        [Fact]
        public void MarkRoomAsCleaning_RoomMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            roomRepositoryMock.Setup(repository => repository.GetById(4)).Returns((ER_Room?)null);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.MarkRoomAsCleaning(4));

            // Assert
            Assert.Equal("Room 4 was not found.", exception.Message);
        }

        [Fact]
        public void MarkRoomAsCleaning_RoomNotOccupied_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 4,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            roomRepositoryMock.Setup(repository => repository.GetById(4)).Returns(room);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.MarkRoomAsCleaning(4));

            // Assert
            Assert.Equal("Room 4 cannot be set to cleaning from 'available'. Must be 'occupied'.", exception.Message);
        }

        [Fact]
        public void MarkRoomAsCleaning_OccupiedRoom_UpdatesStatusAndClearsVisit()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 4,
                Availability_Status = ER_Room.RoomStatus.Occupied
            };

            roomRepositoryMock.Setup(repository => repository.GetById(4)).Returns(room);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            service.MarkRoomAsCleaning(4);

            // Assert
            Assert.Equal(ER_Room.RoomStatus.Cleaning, room.Availability_Status);
            roomRepositoryMock.Verify(repository => repository.UpdateAvailabilityStatus(4, ER_Room.RoomStatus.Cleaning), Times.Once);
            roomRepositoryMock.Verify(repository => repository.ClearCurrentVisit(4), Times.Once);
        }

        [Fact]
        public void MarkRoomAsCleaned_RoomMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            roomRepositoryMock.Setup(repository => repository.GetById(9)).Returns((ER_Room?)null);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.MarkRoomAsCleaned(9));

            // Assert
            Assert.Equal("Room 9 was not found.", exception.Message);
        }

        [Fact]
        public void MarkRoomAsCleaned_RoomNotCleaning_ThrowsInvalidOperationException()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 9,
                Availability_Status = ER_Room.RoomStatus.Occupied
            };

            roomRepositoryMock.Setup(repository => repository.GetById(9)).Returns(room);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.MarkRoomAsCleaned(9));

            // Assert
            Assert.Equal("Room 9 cannot be marked as cleaned — current status is 'occupied', not 'cleaning'.", exception.Message);
        }

        [Fact]
        public void MarkRoomAsCleaned_CleaningRoom_UpdatesStatus()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var room = new ER_Room
            {
                Room_ID = 9,
                Availability_Status = ER_Room.RoomStatus.Cleaning
            };

            roomRepositoryMock.Setup(repository => repository.GetById(9)).Returns(room);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            service.MarkRoomAsCleaned(9);

            // Assert
            Assert.Equal(ER_Room.RoomStatus.Available, room.Availability_Status);
            roomRepositoryMock.Verify(repository => repository.UpdateAvailabilityStatus(9, ER_Room.RoomStatus.Available), Times.Once);
        }

        [Fact]
        public void GetRoomVisitDetails_NoVisitInRoom_ReturnsNull()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            roomRepositoryMock
                .Setup(repository => repository.GetVisitByRoomId(3))
                .Returns((ER_Visit?)null);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetRoomVisitDetails(3);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetRoomVisitDetails_VisitExists_ReturnsBundledDetails()
        {
            // Arrange
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var triageRepositoryMock = new Mock<ITriageRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 11,
                Patient_ID = "123"
            };

            var patient = new Patient
            {
                Patient_ID = "123",
                First_Name = "Ana",
                Last_Name = "Pop"
            };

            var triage = new Triage
            {
                Visit_ID = 11,
                Triage_Level = 2
            };

            roomRepositoryMock
                .Setup(repository => repository.GetVisitByRoomId(3))
                .Returns(visit);

            patientRepositoryMock
                .Setup(repository => repository.GetById("123"))
                .Returns(patient);

            triageRepositoryMock
                .Setup(repository => repository.GetByVisitId(11))
                .Returns(triage);

            var service = new RoomManagementService(
                roomRepositoryMock.Object,
                patientRepositoryMock.Object,
                triageRepositoryMock.Object);

            // Act
            var result = service.GetRoomVisitDetails(3);

            // Assert
            Assert.NotNull(result);
            Assert.Same(visit, result!.Visit);
            Assert.Same(patient, result.Patient);
            Assert.Same(triage, result.Triage);
        }
    }
}