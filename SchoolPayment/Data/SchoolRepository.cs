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

        public IList<School> GetAllSchools()
        {
            var schools = new List<School>();
            const string sql = @"
SELECT Id, Name
FROM dbo.Schools
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

        public string UpsertStudent(Student student)
        {
            const string findSql = @"
SELECT TOP 1 Id
FROM dbo.Students
WHERE SchoolId = @SchoolId
  AND (
        (@Pincode IS NOT NULL AND Pincode = @Pincode)
        OR (@StudentNumber IS NOT NULL AND StudentNumber = @StudentNumber)
      )
ORDER BY CASE WHEN @Pincode IS NOT NULL AND Pincode = @Pincode THEN 0 ELSE 1 END, Id;";

            using (var connection = SqlHelper.CreateConnection())
            {
                connection.Open();
                int? existingId = null;
                using (var find = new SqlCommand(findSql, connection))
                {
                    find.Parameters.Add(SqlHelper.Param("@SchoolId", student.SchoolId, SqlDbType.Int));
                    find.Parameters.Add(SqlHelper.Param("@Pincode", student.Pincode, SqlDbType.NVarChar));
                    find.Parameters.Add(SqlHelper.Param("@StudentNumber", student.StudentNumber, SqlDbType.NVarChar, 50));
                    var found = find.ExecuteScalar();
                    if (found != null && found != DBNull.Value)
                    {
                        existingId = Convert.ToInt32(found);
                    }
                }

                if (existingId.HasValue)
                {
                    const string updateSql = @"
UPDATE dbo.Students
SET FullName = @FullName,
    StudentNumber = @StudentNumber,
    Pincode = @Pincode,
    TotalCost = @TotalCost,
    PaidCost = @PaidCost,
    RemainCost = @RemainCost,
    DebtCost = @DebtCost,
    DiscountCost = @DiscountCost
WHERE Id = @Id";

                    using (var update = new SqlCommand(updateSql, connection))
                    {
                        update.Parameters.Add(SqlHelper.Param("@Id", existingId.Value, SqlDbType.Int));
                        AddStudentParameters(update, student);
                        update.ExecuteNonQuery();
                    }

                    return "updated";
                }

                const string insertSql = @"
INSERT INTO dbo.Students
    (SchoolId, FullName, StudentNumber, Pincode, TotalCost, PaidCost, RemainCost, DebtCost, DiscountCost)
VALUES
    (@SchoolId, @FullName, @StudentNumber, @Pincode, @TotalCost, @PaidCost, @RemainCost, @DebtCost, @DiscountCost)";

                using (var insert = new SqlCommand(insertSql, connection))
                {
                    insert.Parameters.Add(SqlHelper.Param("@SchoolId", student.SchoolId, SqlDbType.Int));
                    AddStudentParameters(insert, student);
                    insert.ExecuteNonQuery();
                }

                return "inserted";
            }
        }

        private static void AddStudentParameters(SqlCommand command, Student student)
        {
            command.Parameters.Add(SqlHelper.Param("@FullName", student.FullName, SqlDbType.NVarChar, 200));
            command.Parameters.Add(SqlHelper.Param("@StudentNumber", student.StudentNumber, SqlDbType.NVarChar, 50));
            command.Parameters.Add(SqlHelper.Param("@Pincode", student.Pincode, SqlDbType.NVarChar));
            command.Parameters.Add(AmountParam("@TotalCost", student.TotalCost));
            command.Parameters.Add(AmountParam("@PaidCost", student.PaidCost));
            command.Parameters.Add(AmountParam("@RemainCost", student.RemainCost));
            command.Parameters.Add(AmountParam("@DebtCost", student.DebtCost));
            command.Parameters.Add(AmountParam("@DiscountCost", student.DiscountCost));
        }

        private static SqlParameter AmountParam(string name, decimal value)
        {
            return new SqlParameter(name, SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = value
            };
        }

        public IList<Student> SearchStudents(int schoolId, string pincode)
        {
            var students = new List<Student>();
            if (schoolId <= 0 || string.IsNullOrWhiteSpace(pincode))
            {
                return students;
            }

            const string sql = @"
SELECT s.Id, s.SchoolId, s.FullName, s.StudentNumber, s.Pincode,
       s.TotalCost, s.PaidCost, s.RemainCost, s.DebtCost, s.DiscountCost
FROM dbo.Students s
WHERE s.SchoolId = @SchoolId
  AND LTRIM(RTRIM(s.Pincode)) = @Pincode
ORDER BY s.FullName";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@Pincode", pincode.Trim(), SqlDbType.NVarChar));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(MapStudent(reader));
                    }
                }
            }

            return students;
        }

        public Student GetStudent(int studentId)
        {
            const string sql = @"
SELECT s.Id, s.SchoolId, s.FullName, s.StudentNumber, s.Pincode,
       s.TotalCost, s.PaidCost, s.RemainCost, s.DebtCost, s.DiscountCost
FROM dbo.Students s
WHERE s.Id = @Id";

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

                    return MapStudent(reader);
                }
            }
        }

        private static Student MapStudent(SqlDataReader reader)
        {
            return new Student
            {
                Id = reader.GetInt32(0),
                SchoolId = reader.GetInt32(1),
                FullName = reader.GetString(2),
                StudentNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                Pincode = reader.IsDBNull(4) ? null : reader.GetString(4),
                TotalCost = reader.GetDecimal(5),
                PaidCost = reader.GetDecimal(6),
                RemainCost = reader.GetDecimal(7),
                DebtCost = reader.GetDecimal(8),
                DiscountCost = reader.GetDecimal(9)
            };
        }
    }
}
