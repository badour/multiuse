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

        public IList<Student> SearchStudents(int schoolId, string studentLookup)
        {
            var students = new List<Student>();
            if (schoolId <= 0 || string.IsNullOrWhiteSpace(studentLookup))
            {
                return students;
            }

            int parsedId;
            var hasNumericId = int.TryParse(studentLookup.Trim(), out parsedId);

            const string sql = @"
SELECT s.Id, s.SchoolId, s.FullName, s.StudentNumber, s.Pincode,
       s.TotalCost, s.PaidCost, s.RemainCost, s.DebtCost, s.DiscountCost
FROM dbo.Students s
WHERE s.SchoolId = @SchoolId
  AND (
        s.StudentNumber = @Lookup
        OR (@HasNumericId = 1 AND s.Id = @StudentId)
      )
ORDER BY s.FullName";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@Lookup", studentLookup.Trim(), SqlDbType.NVarChar, 50));
                command.Parameters.Add(SqlHelper.Param("@HasNumericId", hasNumericId, SqlDbType.Bit));
                command.Parameters.Add(SqlHelper.Param("@StudentId", hasNumericId ? (object)parsedId : 0, SqlDbType.Int));
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
