// File: Unit/ServicesTests/StateManagementServiceAdditionalTests.cs
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;

using Moq;
using System;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class StateManagementServiceAdditionalTests
    {
        [Fact]
        public void ChangeVisitStatus_ClosedVisitWithoutRoomIds_UpdatesVisitAndSkipsRoomCleanup()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 41,
                Status = ER_Visit.VisitStatus.IN_EXAMINATION
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(41))
                .Returns(visit);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByVisitId(41))
                .Returns((int?)null);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByCurrentVisit(41))
                .Returns((int?)null);

            var service = new StateManagementService(
                visitRepositoryMock.Object,
                roomRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(41, ER_Visit.VisitStatus.CLOSED);

            // Assert
            Assert.Equal(ER_Visit.VisitStatus.CLOSED, visit.Status);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(41, ER_Visit.VisitStatus.CLOSED),
                Times.Once);

            roomRepositoryMock.Verify(
                repository => repository.UpdateAvailabilityStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);

            roomRepositoryMock.Verify(
                repository => repository.ClearCurrentVisit(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void ChangeVisitStatus_TransferredVisitWithRoomFoundButNotOccupied_DoesNotUpdateRoom()
        {
            // Arrange
            var visitRepositoryMock = new Mock<IERVisitRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();

            var visit = new ER_Visit
            {
                Visit_ID = 42,
                Status = ER_Visit.VisitStatus.IN_EXAMINATION
            };

            var room = new ER_Room
            {
                Room_ID = 8,
                Availability_Status = ER_Room.RoomStatus.Cleaning,
                Current_Visit_ID = 42
            };

            visitRepositoryMock
                .Setup(repository => repository.GetByVisitId(42))
                .Returns(visit);

            roomRepositoryMock
                .Setup(repository => repository.GetRoomIdByVisitId(42))
                .Returns(8);

            roomRepositoryMock
                .Setup(repository => repository.GetById(8))
                .Returns(room);

            var service = new StateManagementService(
                visitRepositoryMock.Object,
                roomRepositoryMock.Object);

            // Act
            service.ChangeVisitStatus(42, ER_Visit.VisitStatus.TRANSFERRED);

            // Assert
            Assert.Equal(ER_Visit.VisitStatus.TRANSFERRED, visit.Status);

            visitRepositoryMock.Verify(
                repository => repository.UpdateStatus(42, ER_Visit.VisitStatus.TRANSFERRED),
                Times.Once);

            roomRepositoryMock.Verify(
                repository => repository.UpdateAvailabilityStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);

            roomRepositoryMock.Verify(
                repository => repository.ClearCurrentVisit(It.IsAny<int>()),
                Times.Never);
        }
    }
}