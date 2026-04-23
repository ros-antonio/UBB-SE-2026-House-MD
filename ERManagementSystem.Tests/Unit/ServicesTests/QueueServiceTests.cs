using System;
using System.Collections.Generic;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;

using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class QueueServiceTests
    {
        [Fact]
        public void GetOrderedQueue_MultipleVisits_ReturnsOrderedByTriageThenArrivalTime()
        {
            // Arrange
            var repositoryMock = new Mock<IERVisitRepository>();

            var visit1 = new ER_Visit
            {
                Visit_ID = 1,
                Patient_ID = "P1",
                Arrival_date_time = new DateTime(2026, 4, 22, 10, 30, 0),
                Chief_Complaint = "Complaint 1",
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            var visit2 = new ER_Visit
            {
                Visit_ID = 2,
                Patient_ID = "P2",
                Arrival_date_time = new DateTime(2026, 4, 22, 9, 45, 0),
                Chief_Complaint = "Complaint 2",
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            var visit3 = new ER_Visit
            {
                Visit_ID = 3,
                Patient_ID = "P3",
                Arrival_date_time = new DateTime(2026, 4, 22, 9, 15, 0),
                Chief_Complaint = "Complaint 3",
                Status = ER_Visit.VisitStatus.TRIAGED
            };

            var triage1 = new Triage { Visit_ID = 1, Triage_Level = 3, Specialization = "General" };
            var triage2 = new Triage { Visit_ID = 2, Triage_Level = 2, Specialization = "General" };
            var triage3 = new Triage { Visit_ID = 3, Triage_Level = 2, Specialization = "General" };

            var unorderedQueue = new List<(ER_Visit visit, Triage triage)>
            {
                (visit1, triage1),
                (visit2, triage2),
                (visit3, triage3)
            };

            repositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Returns(unorderedQueue);

            var service = new QueueService(repositoryMock.Object);

            // Act
            var result = service.GetOrderedQueue();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0].visit.Visit_ID); // triage 2, earlier arrival
            Assert.Equal(2, result[1].visit.Visit_ID); // triage 2, later arrival
            Assert.Equal(1, result[2].visit.Visit_ID); // triage 3
        }

        [Fact]
        public void GetOrderedQueue_RepositoryThrows_RethrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<IERVisitRepository>();

            repositoryMock
                .Setup(repository => repository.GetActiveVisitsWithTriage())
                .Throws(new Exception("Database failure"));

            var service = new QueueService(repositoryMock.Object);

            // Act
            var exception = Assert.Throws<Exception>(() => service.GetOrderedQueue());

            // Assert
            Assert.Equal("Database failure", exception.Message);
        }

        [Fact]
        public void RemoveFromQueue_VisitExists_UpdatesStatusToInRoom()
        {
            // Arrange
            var repositoryMock = new Mock<IERVisitRepository>();
            var visitId = 10;

            var visit = new ER_Visit
            {
                Visit_ID = visitId,
                Patient_ID = "P10",
                Arrival_date_time = new DateTime(2026, 4, 22, 8, 0, 0),
                Chief_Complaint = "Pain",
                Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM
            };

            repositoryMock
                .Setup(repository => repository.GetByVisitId(visitId))
                .Returns(visit);

            var service = new QueueService(repositoryMock.Object);

            // Act
            service.RemoveFromQueue(visitId);

            // Assert
            var expectedVisit = new ER_Visit
            {
                Visit_ID = visitId,
                Patient_ID = "P10",
                Arrival_date_time = new DateTime(2026, 4, 22, 8, 0, 0),
                Chief_Complaint = "Pain",
                Status = ER_Visit.VisitStatus.IN_ROOM
            };

            Assert.Equivalent(expectedVisit, visit, strict: true);

            repositoryMock.Verify(
                repository => repository.UpdateStatus(visitId, ER_Visit.VisitStatus.IN_ROOM),
                Times.Once);
        }

        [Fact]
        public void RemoveFromQueue_VisitDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var repositoryMock = new Mock<IERVisitRepository>();
            var visitId = 99;

            repositoryMock
                .Setup(repository => repository.GetByVisitId(visitId))
                .Returns((ER_Visit?)null);

            var service = new QueueService(repositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => service.RemoveFromQueue(visitId));

            // Assert
            Assert.Equal($"Visit {visitId} not found.", exception.Message);

            repositoryMock.Verify(
                repository => repository.UpdateStatus(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}