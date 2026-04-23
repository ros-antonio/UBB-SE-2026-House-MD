using System;
using System.Data;
using System.Transactions;
using ERManagementSystem.Core.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ERManagementSystem.Tests.Integration.RepositoriesTests
{
    public class PatientRepositoryIntegrationTests
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=ERManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;";

        [Fact]
        public void Add_ThenGetById_WithTransactionScope_PersistsWithinTransactionAndRollsBack()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var patientId = $"IT{Guid.NewGuid():N}"[..20];
                var expected = new Patient
                {
                    Patient_ID = patientId,
                    First_Name = "Integration",
                    Last_Name = "Test",
                    Date_of_Birth = new DateTime(2000, 1, 1),
                    Gender = "Male",
                    Phone = "0712345678",
                    Emergency_Contact = "Emergency Contact",
                    Transferred = false
                };

                // Act
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                InsertPatient(connection, expected);
                var actual = GetPatientById(connection, patientId);

                // Assert
                Assert.NotNull(actual);
                Assert.Equivalent(expected, actual, strict: true);
            }
        }

        [Fact]
        public void Update_WithTransactionScope_ChangesPatientDataAndRollsBack()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var patientId = $"IT{Guid.NewGuid():N}"[..20];
                var initial = new Patient
                {
                    Patient_ID = patientId,
                    First_Name = "Initial",
                    Last_Name = "Patient",
                    Date_of_Birth = new DateTime(1999, 5, 10),
                    Gender = "Female",
                    Phone = "0700000000",
                    Emergency_Contact = "Initial Contact",
                    Transferred = false
                };

                var updated = new Patient
                {
                    Patient_ID = patientId,
                    First_Name = "Updated",
                    Last_Name = "Patient",
                    Date_of_Birth = new DateTime(1999, 5, 10),
                    Gender = "Female",
                    Phone = "0799999999",
                    Emergency_Contact = "Updated Contact",
                    Transferred = true
                };

                // Act
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                InsertPatient(connection, initial);
                UpdatePatient(connection, updated);
                var actual = GetPatientById(connection, patientId);

                // Assert
                Assert.NotNull(actual);
                Assert.Equivalent(updated, actual, strict: true);

            
            }
        }

        [Fact]
        public void Delete_WithTransactionScope_RemovesPatientWithinTransactionAndRollsBack()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var patientId = $"IT{Guid.NewGuid():N}"[..20];
                var patient = new Patient
                {
                    Patient_ID = patientId,
                    First_Name = "Delete",
                    Last_Name = "Candidate",
                    Date_of_Birth = new DateTime(1995, 7, 20),
                    Gender = "Male",
                    Phone = "0711111111",
                    Emergency_Contact = "Delete Contact",
                    Transferred = false
                };

                // Act
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                InsertPatient(connection, patient);
                DeletePatient(connection, patientId);
                var actual = GetPatientById(connection, patientId);

                // Assert
                Assert.Null(actual);

         
            }
        }
        [Fact]
        public void GetPatientById_NonExistentId_WithTransactionScope_ReturnsNull()
        {
            // Arrange
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var nonExistentPatientId = $"MISSING{Guid.NewGuid():N}"[..20];

                // Act
                var result = GetPatientById(connection, nonExistentPatientId);

                // Assert
                Assert.Null(result);

               
            }
        }

        private static void InsertPatient(SqlConnection connection, Patient patient)
        {
            using var insertCommand = new SqlCommand(
                @"INSERT INTO dbo.Patient
                    (Patient_ID, First_Name, Last_Name, Date_of_Birth, Gender, Phone, Emergency_Contact, Transferred)
                  VALUES
                    (@Patient_ID, @First_Name, @Last_Name, @Date_of_Birth, @Gender, @Phone, @Emergency_Contact, @Transferred)",
                connection);

            insertCommand.Parameters.AddRange(
            [
                new SqlParameter("@Patient_ID", patient.Patient_ID),
                new SqlParameter("@First_Name", patient.First_Name),
                new SqlParameter("@Last_Name", patient.Last_Name),
                new SqlParameter("@Date_of_Birth", patient.Date_of_Birth),
                new SqlParameter("@Gender", patient.Gender),
                new SqlParameter("@Phone", patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred", patient.Transferred)
            ]);

            insertCommand.ExecuteNonQuery();
        }

        private static void UpdatePatient(SqlConnection connection, Patient patient)
        {
            using var updateCommand = new SqlCommand(
                @"UPDATE dbo.Patient
                  SET First_Name = @First_Name,
                      Last_Name = @Last_Name,
                      Date_of_Birth = @Date_of_Birth,
                      Gender = @Gender,
                      Phone = @Phone,
                      Emergency_Contact = @Emergency_Contact,
                      Transferred = @Transferred
                  WHERE Patient_ID = @Patient_ID",
                connection);

            updateCommand.Parameters.AddRange(
            [
                new SqlParameter("@Patient_ID", patient.Patient_ID),
                new SqlParameter("@First_Name", patient.First_Name),
                new SqlParameter("@Last_Name", patient.Last_Name),
                new SqlParameter("@Date_of_Birth", patient.Date_of_Birth),
                new SqlParameter("@Gender", patient.Gender),
                new SqlParameter("@Phone", patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred", patient.Transferred)
            ]);

            updateCommand.ExecuteNonQuery();
        }

        private static void DeletePatient(SqlConnection connection, string patientId)
        {
            using var deleteCommand = new SqlCommand(
                @"DELETE FROM dbo.Patient
                  WHERE Patient_ID = @Patient_ID",
                connection);

            deleteCommand.Parameters.Add(new SqlParameter("@Patient_ID", patientId));
            deleteCommand.ExecuteNonQuery();
        }

        private static Patient? GetPatientById(SqlConnection connection, string patientId)
        {
            using var selectCommand = new SqlCommand(
                @"SELECT Patient_ID, First_Name, Last_Name, Date_of_Birth, Gender, Phone, Emergency_Contact, Transferred
                  FROM dbo.Patient
                  WHERE Patient_ID = @Patient_ID",
                connection);

            selectCommand.Parameters.Add(new SqlParameter("@Patient_ID", patientId));

            using var reader = selectCommand.ExecuteReader(CommandBehavior.SingleRow);
            if (!reader.Read())
            {
                return null;
            }

            return new Patient
            {
                Patient_ID = reader["Patient_ID"] as string ?? string.Empty,
                First_Name = reader["First_Name"] as string ?? string.Empty,
                Last_Name = reader["Last_Name"] as string ?? string.Empty,
                Date_of_Birth = Convert.ToDateTime(reader["Date_of_Birth"]),
                Gender = reader["Gender"] as string ?? string.Empty,
                Phone = reader["Phone"] as string ?? string.Empty,
                Emergency_Contact = reader["Emergency_Contact"] as string ?? string.Empty,
                Transferred = Convert.ToBoolean(reader["Transferred"])
            };
        }
    }
}
