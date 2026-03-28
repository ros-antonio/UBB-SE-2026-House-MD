using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;

namespace ERManagementSystem.Repositories
{
    public class ExaminationRepository
    {
        private readonly SqlHelper _sqlHelper;

        public ExaminationRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// Inserts a new Examination record into the database.
        public void Add(Examination exam)
        {
            string sql = @"INSERT INTO Examination (Exam_ID, Visit_ID, Doctor_ID, Exam_Time, Room_ID, Notes)
                           VALUES (@Exam_ID, @Visit_ID, @Doctor_ID, @Exam_Time, @Room_ID, @Notes)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Exam_ID", exam.Visit_ID),
                new SqlParameter("@Visit_ID", exam.Visit_ID),
                new SqlParameter("@Doctor_ID", exam.Doctor_ID),
                new SqlParameter("@Exam_time", exam.Exam_Time),
                new SqlParameter("@Room_ID", exam.Room_ID),
                new SqlParameter("@Notes", exam.Notes)
            };

            _sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        /// Retrieves an Examination record by Visit_ID.
        /// Returns null if no record is found.
        public Examination? GetByVisitId(int visitId)
        {
            string sql = "SELECT Exam_ID, Visit_ID, Doctor_ID, Exam_Time, Room_ID, Notes FROM Examination WHERE Visit_ID = @Visit_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", visitId)
            };

            using var reader = _sqlHelper.ExecuteReader(sql, parameters);

            if (reader.Read())
            {
                return new Examination
                {
                    Exam_ID = reader.GetInt32(reader.GetOrdinal("Exam_ID")),
                    Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                    Doctor_ID = reader.GetInt32(reader.GetOrdinal("Doctor_ID")),
                    Exam_Time = reader.GetDateTime(reader.GetOrdinal("Exam_time")),
                    Room_ID = reader.GetInt32(reader.GetOrdinal("Room_ID")),
                    Notes = reader.GetString(reader.GetOrdinal("Notes"))
                };
            }

            return null;
        }

        public void Delete(Examination exam)
        {
            const string query = @"
                DELETE FROM dbo.Examination
                WHERE Exam_ID = @Exam_ID";

            var parameters = new[]
            {
                new SqlParameter("@Exam_ID", exam.Exam_ID)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }
    }
}
