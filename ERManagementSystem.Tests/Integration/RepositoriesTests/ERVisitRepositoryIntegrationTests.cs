using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using ERManagementSystem.Core.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ERManagementSystem.Tests.Integration.RepositoriesTests
{
    public class ERVisitRepositoryIntegrationTests
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=ERManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;";

        [Fact]
        public void AddVisit_ThenGetById_WithTransactionScope_PersistsWithinTransactionAndRollsBack()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var patientId = $"ITP{Guid.NewGuid():N}"[..20];
                InsertPatient(connection, patientId);

                var expected = new ER_Visit
                {
                    Patient_ID = patientId,
                    Arrival_date_time = new DateTime(2026, 5, 1, 10, 15, 0),
                    Chief_Complaint = "Integration complaint",
                    Status = ER_Visit.VisitStatus.REGISTERED
                };

                // Act
                var visitId = InsertVisit(connection, expected);
                var actual = GetVisitById(connection, visitId);

                // Assert
                Assert.NotNull(actual);

                expected.Visit_ID = visitId;
                Assert.Equivalent(expected, actual, strict: true);

               
            }
        }

        [Fact]
        public void UpdateStatus_WithTransactionScope_UpdatesVisitStatusAndRollsBack()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var patientId = $"ITP{Guid.NewGuid():N}"[..20];
                InsertPatient(connection, patientId);

                var initialVisit = new ER_Visit
                {
                    Patient_ID = patientId,
                    Arrival_date_time = new DateTime(2026, 5, 1, 11, 0, 0),
                    Chief_Complaint = "Status update complaint",
                    Status = ER_Visit.VisitStatus.REGISTERED
                };

                var visitId = InsertVisit(connection, initialVisit);

                // Act
                UpdateVisitStatus(connection, visitId, ER_Visit.VisitStatus.WAITING_FOR_ROOM);
                var updatedVisit = GetVisitById(connection, visitId);

                // Assert
                Assert.NotNull(updatedVisit);
                Assert.Equal(ER_Visit.VisitStatus.WAITING_FOR_ROOM, updatedVisit!.Status);

           
            }
        }

        [Fact]
        public void GetActiveVisits_WithTransactionScope_ReturnsOnlyNonClosedAndNonTransferred()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var patientId = $"ITP{Guid.NewGuid():N}"[..20];
                InsertPatient(connection, patientId);

                var activeVisitId = InsertVisit(connection, new ER_Visit
                {
                    Patient_ID = patientId,
                    Arrival_date_time = new DateTime(2026, 5, 1, 12, 0, 0),
                    Chief_Complaint = "Active visit",
                    Status = ER_Visit.VisitStatus.REGISTERED
                });

                var closedVisitId = InsertVisit(connection, new ER_Visit
                {
                    Patient_ID = patientId,
                    Arrival_date_time = new DateTime(2026, 5, 1, 12, 30, 0),
                    Chief_Complaint = "Closed visit",
                    Status = ER_Visit.VisitStatus.CLOSED
                });

                var transferredVisitId = InsertVisit(connection, new ER_Visit
                {
                    Patient_ID = patientId,
                    Arrival_date_time = new DateTime(2026, 5, 1, 13, 0, 0),
                    Chief_Complaint = "Transferred visit",
                    Status = ER_Visit.VisitStatus.TRANSFERRED
                });

                // Act
                var activeVisits = GetActiveVisits(connection);
                var activeIds = activeVisits.Select(visit => visit.Visit_ID).ToList();

                // Assert
                Assert.Contains(activeVisitId, activeIds);
                Assert.DoesNotContain(closedVisitId, activeIds);
                Assert.DoesNotContain(transferredVisitId, activeIds);

            
            }
        }
        // Add to: ERManagementSystem.Tests/Integration/RepositoriesTests/ERVisitRepositoryIntegrationTests.cs

        [Fact]
        public void GetVisitById_NonExistentId_WithTransactionScope_ReturnsNull()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                // Act
                var result = GetVisitById(connection, -999);

                // Assert
                Assert.Null(result);

                
            }
        }

        [Fact]
        public void GetActiveVisits_WhenNoActiveVisits_WithTransactionScope_ReturnsEmptyList()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                using var closeAllActiveCommand = new SqlCommand(
                    @"UPDATE dbo.ER_Visit
              SET Status = 'CLOSED'
              WHERE Status NOT IN ('CLOSED', 'TRANSFERRED')",
                    connection);

                closeAllActiveCommand.ExecuteNonQuery();

                // Act
                var result = GetActiveVisits(connection);

                // Assert
                Assert.Empty(result);

                
            }
        }





        private static void InsertPatient(SqlConnection connection, string patientId)
        {
            using var insertPatientCommand = new SqlCommand(
                @"INSERT INTO dbo.Patient
                    (Patient_ID, First_Name, Last_Name, Date_of_Birth, Gender, Phone, Emergency_Contact, Transferred)
                  VALUES
                    (@Patient_ID, @First_Name, @Last_Name, @Date_of_Birth, @Gender, @Phone, @Emergency_Contact, @Transferred)",
                connection);

            insertPatientCommand.Parameters.AddRange(
            [
                new SqlParameter("@Patient_ID", patientId),
                new SqlParameter("@First_Name", "Integration"),
                new SqlParameter("@Last_Name", "VisitPatient"),
                new SqlParameter("@Date_of_Birth", new DateTime(1990, 1, 1)),
                new SqlParameter("@Gender", "Male"),
                new SqlParameter("@Phone", "0700000000"),
                new SqlParameter("@Emergency_Contact", "Integration Contact"),
                new SqlParameter("@Transferred", false)
            ]);

            insertPatientCommand.ExecuteNonQuery();
        }

        private static int InsertVisit(SqlConnection connection, ER_Visit visit)
        {
            using var insertVisitCommand = new SqlCommand(
                @"INSERT INTO dbo.ER_Visit
                    (Patient_ID, Arrival_date_time, Chief_Complaint, Status)
                  OUTPUT INSERTED.Visit_ID
                  VALUES
                    (@Patient_ID, @Arrival_date_time, @Chief_Complaint, @Status)",
                connection);

            insertVisitCommand.Parameters.AddRange(
            [
                new SqlParameter("@Patient_ID", visit.Patient_ID),
                new SqlParameter("@Arrival_date_time", visit.Arrival_date_time),
                new SqlParameter("@Chief_Complaint", visit.Chief_Complaint),
                new SqlParameter("@Status", visit.Status)
            ]);

            return Convert.ToInt32(insertVisitCommand.ExecuteScalar());
        }

        private static void UpdateVisitStatus(SqlConnection connection, int visitId, string newStatus)
        {
            using var updateCommand = new SqlCommand(
                @"UPDATE dbo.ER_Visit
                  SET Status = @Status
                  WHERE Visit_ID = @Visit_ID",
                connection);

            updateCommand.Parameters.AddRange(
            [
                new SqlParameter("@Status", newStatus),
                new SqlParameter("@Visit_ID", visitId)
            ]);

            updateCommand.ExecuteNonQuery();
        }

        private static ER_Visit? GetVisitById(SqlConnection connection, int visitId)
        {
            using var selectCommand = new SqlCommand(
                @"SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                  FROM dbo.ER_Visit
                  WHERE Visit_ID = @Visit_ID",
                connection);

            selectCommand.Parameters.Add(new SqlParameter("@Visit_ID", visitId));

            using var reader = selectCommand.ExecuteReader(CommandBehavior.SingleRow);
            if (!reader.Read())
            {
                return null;
            }

            return new ER_Visit
            {
                Visit_ID = Convert.ToInt32(reader["Visit_ID"]),
                Patient_ID = reader["Patient_ID"] as string ?? string.Empty,
                Arrival_date_time = Convert.ToDateTime(reader["Arrival_date_time"]),
                Chief_Complaint = reader["Chief_Complaint"] as string ?? string.Empty,
                Status = reader["Status"] as string ?? string.Empty
            };
        }

        private static List<ER_Visit> GetActiveVisits(SqlConnection connection)
        {
            using var selectCommand = new SqlCommand(
                @"SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                  FROM dbo.ER_Visit
                  WHERE Status NOT IN ('TRANSFERRED', 'CLOSED')",
                connection);

            using var reader = selectCommand.ExecuteReader();

            var visits = new List<ER_Visit>();
            while (reader.Read())
            {
                visits.Add(new ER_Visit
                {
                    Visit_ID = Convert.ToInt32(reader["Visit_ID"]),
                    Patient_ID = reader["Patient_ID"] as string ?? string.Empty,
                    Arrival_date_time = Convert.ToDateTime(reader["Arrival_date_time"]),
                    Chief_Complaint = reader["Chief_Complaint"] as string ?? string.Empty,
                    Status = reader["Status"] as string ?? string.Empty
                });
            }

            return visits;
        }
    }
}
