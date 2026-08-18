using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SchoolPayment.Models;

namespace SchoolPayment.Data
{
    public class SchoolRepository
    {
        public IList<School> GetSchools()
        {
            var schools = new List<School>();
            const string sql = @"
SELECT Id, Name
FROM dbo.Schools
WHERE IsActive = 1
ORDER BY Name";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schools.Add(new School
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return schools;
        }

        public IList<Stage> GetStagesBySchool(int schoolId)
        {
            var stages = new List<Stage>();
            const string sql = @"
SELECT Id, SchoolId, Name
FROM dbo.Stages
WHERE SchoolId = @SchoolId
ORDER BY SortOrder, Name";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId, SqlDbType.Int));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stages.Add(new Stage
                        {
                            Id = reader.GetInt32(0),
                            SchoolId = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        });
                    }
                }
            }

            return stages;
        }

        public IList<Student> GetStudents(int schoolId, int stageId)
        {
            var students = new List<Student>();
            const string sql = @"
SELECT Id, SchoolId, StageId, FullName, StudentNumber, OutstandingDebt
FROM dbo.Students
WHERE SchoolId = @SchoolId AND StageId = @StageId
ORDER BY FullName";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@StageId", stageId, SqlDbType.Int));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            SchoolId = reader.GetInt32(1),
                            StageId = reader.GetInt32(2),
                            FullName = reader.GetString(3),
                            StudentNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            OutstandingDebt = reader.GetDecimal(5)
                        });
                    }
                }
            }

            return students;
        }

        public Student GetStudent(int studentId)
        {
            const string sql = @"
SELECT Id, SchoolId, StageId, FullName, StudentNumber, OutstandingDebt
FROM dbo.Students
WHERE Id = @Id";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@Id", studentId, SqlDbType.Int));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new Student
                    {
                        Id = reader.GetInt32(0),
                        SchoolId = reader.GetInt32(1),
                        StageId = reader.GetInt32(2),
                        FullName = reader.GetString(3),
                        StudentNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                        OutstandingDebt = reader.GetDecimal(5)
                    };
                }
            }
        }

        public decimal? GetFeeAmount(int schoolId, int stageId, string paymentType)
        {
            const string sql = @"
SELECT Amount
FROM dbo.PaymentFees
WHERE SchoolId = @SchoolId AND StageId = @StageId AND PaymentType = @PaymentType";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@StageId", stageId, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@PaymentType", paymentType, SqlDbType.NVarChar));
                connection.Open();
                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToDecimal(result);
            }
        }

        public decimal? ResolveAmount(int schoolId, int stageId, int studentId, string paymentType)
        {
            if (string.Equals(paymentType, PaymentTypes.Debt, StringComparison.Ordinal))
            {
                var student = GetStudent(studentId);
                return student == null ? (decimal?)null : student.OutstandingDebt;
            }

            return GetFeeAmount(schoolId, stageId, paymentType);
        }
    }
}
