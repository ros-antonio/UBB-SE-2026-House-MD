using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class StateManagementServiceTests
    {
        [Fact]
        public void CanTransitionTo_ValidTransition_ReturnsTrue()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            var result = service.CanTransitionTo(
                ER_Visit.VisitStatus.REGISTERED,
                ER_Visit.VisitStatus.TRIAGED);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanTransitionTo_UnknownCurrentStatus_ReturnsFalse()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            var result = service.CanTransitionTo("UNKNOWN_STATUS", ER_Visit.VisitStatus.CLOSED);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ChangeStatus_ValidTransition_UpdatesVisitStatus()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            var visit = new ER_Visit
            {
                Visit_ID = 1,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            // Act
            service.ChangeStatus(visit, ER_Visit.VisitStatus.TRIAGED);

            // Assert
            Assert.Equal(ER_Visit.VisitStatus.TRIAGED, visit.Status);
        }

        [Fact]
        public void ChangeStatus_InvalidTransition_ThrowsInvalidOperationException()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            var visit = new ER_Visit
            {
                Visit_ID = 2,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => service.ChangeStatus(visit, ER_Visit.VisitStatus.CLOSED));

            // Assert
            Assert.Contains("Invalid transition", exception.Message);
            Assert.Equal(ER_Visit.VisitStatus.REGISTERED, visit.Status);
        }

        [Fact]
        public void ChangeVisitStatus_VisitDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(100))
                .Returns((ER_Visit?)null);

            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => service.ChangeVisitStatus(100, ER_Visit.VisitStatus.TRIAGED));

            // Assert
            Assert.Equal("ER Visit with ID 100 was not found.", exception.Message);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ChangeVisitStatus_ValidTransition_UpdatesRepository()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 5,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(5))
                .Returns(visit);

            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(5, ER_Visit.VisitStatus.TRIAGED);

            // Assert
            Assert.Equal(ER_Visit.VisitStatus.TRIAGED, visit.Status);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(5, ER_Visit.VisitStatus.TRIAGED),
                Times.Once);
        }

        [Fact]
        public void ChangeVisitStatus_InvalidTransition_ThrowsInvalidOperationException()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 6,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(6))
                .Returns(visit);

            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => service.ChangeVisitStatus(6, ER_Visit.VisitStatus.CLOSED));

            // Assert
            Assert.Contains("Invalid transition", exception.Message);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ChangeVisitStatus_ClosedVisitWithOccupiedRoom_SetsRoomToCleaningAndClearsCurrentVisit()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 7,
                Status = ER_Visit.VisitStatus.IN_EXAMINATION
            };

            var room = new ER_Room
            {
                Room_ID = 12,
                Room_Type = ER_Room.RoomType.GeneralRoom,
                Availability_Status = ER_Room.RoomStatus.Occupied,
                Current_Visit_ID = 7
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(7))
                .Returns(visit);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByVisitId(7))
                .Returns(12);

            roomRepositoryMock
                .Setup(repository => repository.GetById(12))
                .Returns(room);

            var service = new StateManagementService(
                visitRepositoryMock.Object,
                roomRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(7, ER_Visit.VisitStatus.CLOSED);

            // Assert
            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(7, ER_Visit.VisitStatus.CLOSED),
                Times.Once);

            roomRepositoryMock.Verify(
                repository => repository.UpdateAvailabilityStatus(12, ER_Room.RoomStatus.Cleaning),
                Times.Once);

            roomRepositoryMock.Verify(
                repository => repository.ClearCurrentVisit(12),
                Times.Once);

            Assert.Equal(ER_Room.RoomStatus.Cleaning, room.Availability_Status);
        }

        [Fact]
        public void ChangeVisitStatus_NoRoomFound_UsesFallbackLookup()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 8,
                Status = ER_Visit.VisitStatus.IN_EXAMINATION
            };

            var room = new ER_Room
            {
                Room_ID = 20,
                Room_Type = ER_Room.RoomType.GeneralRoom,
                Availability_Status = ER_Room.RoomStatus.Occupied,
                Current_Visit_ID = 8
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(8))
                .Returns(visit);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByVisitId(8))
                .Returns((int?)null);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByCurrentVisit(8))
                .Returns(20);

            roomRepositoryMock
                .Setup(repository => repository.GetById(20))
                .Returns(room);

            var service = new StateManagementService(
                visitRepositoryMock.Object,
                roomRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(8, ER_Visit.VisitStatus.CLOSED);

            // Assert
            roomRepositoryMock.Verify(repository => repository.GetRoomIdByVisitId(8), Times.Once);
            roomRepositoryMock.Verify(repository => repository.GetRoomIdByCurrentVisit(8), Times.Once);
            roomRepositoryMock.Verify(repository => repository.UpdateAvailabilityStatus(20, ER_Room.RoomStatus.Cleaning), Times.Once);
            roomRepositoryMock.Verify(repository => repository.ClearCurrentVisit(20), Times.Once);
        }

        [Fact]
        public void ChangeVisitStatus_RoomHookThrows_DoesNotRollbackVisitStatusUpdate()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 9,
                Status = ER_Visit.VisitStatus.IN_EXAMINATION
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(9))
                .Returns(visit);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByVisitId(9))
                .Throws(new Exception("Room lookup failed"));

            var service = new StateManagementService(
                visitRepositoryMock.Object,
                roomRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(9, ER_Visit.VisitStatus.CLOSED);

            // Assert
            Assert.Equal(ER_Visit.VisitStatus.CLOSED, visit.Status);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(9, ER_Visit.VisitStatus.CLOSED),
                Times.Once);
        }

        [Fact]
        public void CanClose_TriagedVisit_ReturnsTrue()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            var visit = new ER_Visit
            {
                Visit_ID = 11,
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            // Act
            var result = service.CanClose(visit);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CloseVisit_AllowedState_ClosesVisit()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 13,
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(13))
                .Returns(visit);

            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            service.CloseVisit(13);

            // Assert
            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(13, ER_Visit.VisitStatus.CLOSED),
                Times.Once);

            Assert.Equal(ER_Visit.VisitStatus.CLOSED, visit.Status);
        }

        [Fact]
        public void CloseVisit_DisallowedState_ThrowsInvalidOperationException()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 14,
                Status = ER_Visit.VisitStatus.REGISTERED
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(14))
                .Returns(visit);

            var service = new StateManagementService(visitRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.CloseVisit(14));

            // Assert
            Assert.Contains("cannot be closed", exception.Message);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ChangeStatus_UnknownCurrentStatus_ThrowsKeyNotFoundException()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var service = new StateManagementService(visitRepositoryMock.Object);

            var visit = new ER_Visit
            {
                Visit_ID = 15,
                Status = "UNKNOWN_STATUS"
            };

            // Act
            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => service.ChangeStatus(visit, ER_Visit.VisitStatus.CLOSED));

            // Assert
            Assert.Contains("UNKNOWN_STATUS", exception.Message);
        }
    }
}
