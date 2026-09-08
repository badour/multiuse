using System.Data;
using System.Data.SqlClient;
using SchoolPayment.Models;

namespace SchoolPayment.Data
{
    public class UserRepository
    {
        public PortalUser GetActiveByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            const string sql = @"
SELECT Id, Username, PasswordHash, DisplayName, IsActive
FROM dbo.PortalUsers
WHERE Username = @Username AND IsActive = 1";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@Username", username.Trim(), SqlDbType.NVarChar, 80));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new PortalUser
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        DisplayName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        IsActive = reader.GetBoolean(4)
                    };
                }
            }
        }
    }
}
