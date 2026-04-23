// File: Unit/ModelsTests/QueueItemDisplayTests.cs
using System;
using ERManagementSystem.Core.Models;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ModelsTests
{
    public class QueueItemDisplayTests
    {
        [Fact]
        public void Constructor_VisitAndTriage_MapsFieldsCorrectly()
        {
            // Arrange
            var arrival = new DateTime(2026, 4, 23, 12, 0, 0);

            var visit = new ER_Visit
            {
                Visit_ID = 7,
                Patient_ID = "1234567890123",
                Arrival_date_time = arrival,
                Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM
            };

            var triage = new Triage
            {
                Triage_ID = 8,
                Visit_ID = 7,
                Triage_Level = 2,
                Specialization = "Orthopedics"
            };

            // Act
            var result = new QueueItemDisplay(visit, triage);

            // Assert
            Assert.Equal(7, result.VisitId);
            Assert.Equal("1234567890123", result.PatientId);
            Assert.Equal(2, result.TriageLevel);
            Assert.Equal("Orthopedics", result.Specialization);
            Assert.Equal(arrival, result.ArrivalTime);
            Assert.Equal(ER_Visit.VisitStatus.WAITING_FOR_ROOM, result.Status);
        }
    }
}