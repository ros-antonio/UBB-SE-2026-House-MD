// File: Unit/HelpersTests/RoomTypeHelperTests.cs
using ERManagementSystem.Core.Helpers;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.HelpersTests
{
    public class RoomTypeHelperTests
    {
        [Fact]
        public void DetermineRoomType_SpecializationGeneralSurgery_ReturnsOperatingRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "General Surgery",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.OperatingRoom, result);
        }

        [Fact]
        public void DetermineRoomType_SpecializationNeurology_ReturnsNeurologyRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Neurology",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.NeurologyRoom, result);
        }

        [Fact]
        public void DetermineRoomType_SpecializationPulmonology_ReturnsRespiratoryRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Pulmonology",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.RespiratoryRoom, result);
        }

        [Fact]
        public void DetermineRoomType_SpecializationOrthopedics_ReturnsOrthopedicRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Orthopedics",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.OrthopedicRoom, result);
        }

        [Fact]
        public void DetermineRoomType_CriticalConsciousnessWithoutStrictSpecialization_ReturnsTraumaBay()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Emergency Medicine",
                bleeding: 1,
                injuryType: 1,
                consciousness: 3,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.TraumaBay, result);
        }

        [Fact]
        public void DetermineRoomType_CriticalBreathingWithoutStrictSpecialization_ReturnsTraumaBay()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                null,
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 3);

            // Assert
            Assert.Equal(ER_Room.RoomType.TraumaBay, result);
        }

        [Fact]
        public void DetermineRoomType_ConsciousnessTwoWithoutHigherPriorityRules_ReturnsNeurologyRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Emergency Medicine",
                bleeding: 1,
                injuryType: 1,
                consciousness: 2,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.NeurologyRoom, result);
        }

        [Fact]
        public void DetermineRoomType_BreathingTwoWithoutHigherPriorityRules_ReturnsRespiratoryRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Emergency Medicine",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 2);

            // Assert
            Assert.Equal(ER_Room.RoomType.RespiratoryRoom, result);
        }

        [Fact]
        public void DetermineRoomType_InjuryTypeTwoWithoutHigherPriorityRules_ReturnsOrthopedicRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Emergency Medicine",
                bleeding: 1,
                injuryType: 2,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.OrthopedicRoom, result);
        }

        [Fact]
        public void DetermineRoomType_NoRuleMatches_ReturnsGeneralRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Emergency Medicine",
                bleeding: 1,
                injuryType: 1,
                consciousness: 1,
                breathing: 1);

            // Assert
            Assert.Equal(ER_Room.RoomType.GeneralRoom, result);
        }

        [Fact]
        public void DetermineRoomType_StrictSpecializationHasPriorityOverCriticalVitals_ReturnsSpecializationRoom()
        {
            // Arrange

            // Act
            var result = RoomTypeHelper.DetermineRoomType(
                "Orthopedics",
                bleeding: 3,
                injuryType: 3,
                consciousness: 3,
                breathing: 3);

            // Assert
            Assert.Equal(ER_Room.RoomType.OrthopedicRoom, result);
        }
    }
}