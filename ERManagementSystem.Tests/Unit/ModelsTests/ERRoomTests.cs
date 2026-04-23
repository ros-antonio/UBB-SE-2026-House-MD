// File: Unit/ModelsTests/ERRoomTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class ERRoomTests
    {
        [Fact]
        public void UpdateAvailabilityStatus_ValidTransitionFromAvailableToOccupied_UpdatesStatus()
        {
            // Arrange
            var room = new ER_Room
            {
                Room_ID = 5,
                Room_Type = ER_Room.RoomType.GeneralRoom,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            // Act
            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Occupied);

            // Assert
            Assert.Equal(ER_Room.RoomStatus.Occupied, room.Availability_Status);
        }

        [Fact]
        public void UpdateAvailabilityStatus_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var room = new ER_Room
            {
                Room_ID = 5,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            // Act
            var exception = Assert.Throws<ArgumentException>(() => room.UpdateAvailabilityStatus("broken"));

            // Assert
            Assert.Contains("'broken' is not a valid room status.", exception.Message);
        }

        [Fact]
        public void UpdateAvailabilityStatus_InvalidTransition_ThrowsInvalidOperationException()
        {
            // Arrange
            var room = new ER_Room
            {
                Room_ID = 5,
                Availability_Status = ER_Room.RoomStatus.Available
            };

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning));

            // Assert
            Assert.Contains("Invalid room status transition", exception.Message);
        }

        [Fact]
        public void ToString_ValidRoom_ReturnsExpectedFormat()
        {
            // Arrange
            var room = new ER_Room
            {
                Room_ID = 7,
                Room_Type = ER_Room.RoomType.OrthopedicRoom,
                Availability_Status = ER_Room.RoomStatus.Cleaning
            };

            // Act
            var result = room.ToString();

            // Assert
            Assert.Equal("[Room 7] Type: Orthopedic/Procedure Room | Status: cleaning", result);
        }
    }
}