using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;

namespace ERManagementSystem.Repositories
{
    public class TriageRepository
    {
        private readonly SqlHelper _sqlHelper;

        public TriageRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// <summary>
        /// Inserts a new Triage record into the database.
        /// </summary>
        public void Add(Triage triage)
        {
            string sql = @"INSERT INTO Triage (Visit_ID, Triage_Level, Specialization, Nurse_ID, Triage_Time)
                           VALUES (@Visit_ID, @Triage_Level, @Specialization, @Nurse_ID, @Triage_Time)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", triage.Visit_ID),
                new SqlParameter("@Triage_Level", triage.Triage_Level),
                new SqlParameter("@Specialization", triage.Specialization),
                new SqlParameter("@Nurse_ID", triage.Nurse_ID),
                new SqlParameter("@Triage_Time", triage.Triage_Time)
            };

            _sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        public int AddAndReturnId(Triage triage)
        {
            string sql = @"
            INSERT INTO Triage (Visit_ID, Triage_Level, Specialization, Nurse_ID, Triage_Time)
            OUTPUT INSERTED.Triage_ID
            VALUES (@Visit_ID, @Triage_Level, @Specialization, @Nurse_ID, @Triage_Time)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", triage.Visit_ID),
                new SqlParameter("@Triage_Level", triage.Triage_Level),
                new SqlParameter("@Specialization", triage.Specialization),
                new SqlParameter("@Nurse_ID", triage.Nurse_ID),
                new SqlParameter("@Triage_Time", triage.Triage_Time)
            };

            // Use ExecuteReader instead of ExecuteScalar
            using var reader = _sqlHelper.ExecuteReader(sql, parameters);
            if (reader.Read())
            {
                return reader.GetInt32(reader.GetOrdinal("Triage_ID"));
            }

            throw new InvalidOperationException("Failed to insert Triage and retrieve ID.");
        }

        /// <summary>
        /// Retrieves a Triage record by Visit_ID.
        /// Returns null if no record is found.
        /// </summary>
        public Triage? GetByVisitId(int visitId)
        {
            string sql = "SELECT Triage_ID, Visit_ID, Triage_Level, Specialization, Nurse_ID, Triage_Time FROM Triage WHERE Visit_ID = @Visit_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", visitId)
            };

            using var reader = _sqlHelper.ExecuteReader(sql, parameters);

            if (reader.Read())
            {
                return new Triage
                {
                    Triage_ID = reader.GetInt32(reader.GetOrdinal("Triage_ID")),
                    Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                    Triage_Level = reader.GetInt32(reader.GetOrdinal("Triage_Level")),
                    Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
                    Nurse_ID = reader.GetInt32(reader.GetOrdinal("Nurse_ID")),
                    Triage_Time = reader.GetDateTime(reader.GetOrdinal("Triage_Time"))
                };
            }

            return null;
        }

        public void Delete(Triage triage)
        {
            if (triage == null || triage.Triage_ID <= 0)
                throw new ArgumentException("Invalid Triage object.");

            string sql = "DELETE FROM Triage WHERE Triage_ID = @Triage_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", triage.Triage_ID)
            };

            _sqlHelper.ExecuteNonQuery(sql, parameters);
        }
    }
}
