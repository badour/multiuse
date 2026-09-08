using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SchoolPayment.Models;
using SchoolPayment.Services;

namespace SchoolPayment.Data
{
    public class PaymentRepository
    {
        public void Insert(PaymentRecord payment)
        {
            const string sql = @"
INSERT INTO dbo.Payments
(
    OrderId, SchoolId, StudentId, PaymentType, Amount, Currency,
    PayerName, PayerEmail, PayerPhone, AlqasehPaymentId, PaymentToken, Status, GatewayStatus
)
VALUES
(
    @OrderId, @SchoolId, @StudentId, @PaymentType, @Amount, @Currency,
    @PayerName, @PayerEmail, @PayerPhone, @AlqasehPaymentId, @PaymentToken, @Status, @GatewayStatus
)";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddCommonParameters(command, payment);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateGatewayIdentifiers(string orderId, string paymentId, string token, string status)
        {
            const string sql = @"
UPDATE dbo.Payments
SET AlqasehPaymentId = @AlqasehPaymentId,
    PaymentToken = @PaymentToken,
    Status = @Status,
    UpdatedAt = SYSUTCDATETIME()
WHERE OrderId = @OrderId";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@OrderId", orderId, SqlDbType.Char, 32));
                command.Parameters.Add(SqlHelper.Param("@AlqasehPaymentId", paymentId, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@PaymentToken", token, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@Status", status, SqlDbType.NVarChar));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateStatus(string orderId, string status, string gatewayStatus, string approvalCode, string rrn)
        {
            const string sql = @"
UPDATE dbo.Payments
SET Status = @Status,
    GatewayStatus = @GatewayStatus,
    ApprovalCode = COALESCE(@ApprovalCode, ApprovalCode),
    Rrn = COALESCE(@Rrn, Rrn),
    UpdatedAt = SYSUTCDATETIME()
WHERE OrderId = @OrderId";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@OrderId", orderId, SqlDbType.Char, 32));
                command.Parameters.Add(SqlHelper.Param("@Status", status, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@GatewayStatus", gatewayStatus, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@ApprovalCode", approvalCode, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@Rrn", rrn, SqlDbType.NVarChar));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateStatusByPaymentId(string paymentId, string orderId, string status, string gatewayStatus, string approvalCode, string rrn)
        {
            const string sql = @"
UPDATE dbo.Payments
SET Status = @Status,
    GatewayStatus = @GatewayStatus,
    ApprovalCode = COALESCE(@ApprovalCode, ApprovalCode),
    Rrn = COALESCE(@Rrn, Rrn),
    AlqasehPaymentId = COALESCE(@AlqasehPaymentId, AlqasehPaymentId),
    UpdatedAt = SYSUTCDATETIME()
WHERE (@OrderId IS NOT NULL AND OrderId = @OrderId)
   OR (@AlqasehPaymentId IS NOT NULL AND AlqasehPaymentId = @AlqasehPaymentId)";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@OrderId", string.IsNullOrWhiteSpace(orderId) ? (object)null : orderId, SqlDbType.Char, 32));
                command.Parameters.Add(SqlHelper.Param("@AlqasehPaymentId", string.IsNullOrWhiteSpace(paymentId) ? (object)null : paymentId, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@Status", status, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@GatewayStatus", gatewayStatus, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@ApprovalCode", approvalCode, SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@Rrn", rrn, SqlDbType.NVarChar));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public PaymentRecord GetByOrderId(string orderId)
        {
            const string sql = @"
SELECT p.Id, p.OrderId, p.SchoolId, p.StudentId,
       sc.Name, s.FullName,
       p.PaymentType, p.Amount, p.Currency, p.PayerName, p.PayerEmail, p.PayerPhone,
       p.AlqasehPaymentId, p.PaymentToken, p.Status, p.GatewayStatus, p.ApprovalCode, p.Rrn, p.CreatedAt
FROM dbo.Payments p
INNER JOIN dbo.Schools sc ON sc.Id = p.SchoolId
INNER JOIN dbo.Students s ON s.Id = p.StudentId
WHERE p.OrderId = @OrderId";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@OrderId", orderId, SqlDbType.Char, 32));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return Map(reader);
                }
            }
        }

        public IList<PaymentRecord> Search(DateTime? fromDate, DateTime? toDate, string pincode, string statusGroup, int? schoolId)
        {
            var rows = new List<PaymentRecord>();
            const string sql = @"
SELECT p.Id, p.OrderId, p.SchoolId, p.StudentId,
       sc.Name, s.FullName, s.Pincode,
       p.PaymentType, p.Amount, p.Currency, p.PayerName, p.PayerEmail, p.PayerPhone,
       p.AlqasehPaymentId, p.PaymentToken, p.Status, p.GatewayStatus, p.ApprovalCode, p.Rrn, p.CreatedAt
FROM dbo.Payments p
INNER JOIN dbo.Schools sc ON sc.Id = p.SchoolId
INNER JOIN dbo.Students s ON s.Id = p.StudentId
WHERE (@FromDate IS NULL OR CAST(p.CreatedAt AS DATE) >= @FromDate)
  AND (@ToDate IS NULL OR CAST(p.CreatedAt AS DATE) <= @ToDate)
  AND (@Pincode IS NULL OR LTRIM(RTRIM(s.Pincode)) = @Pincode)
  AND (@SchoolId IS NULL OR p.SchoolId = @SchoolId)
  AND (
        @StatusGroup IS NULL
        OR (@StatusGroup = N'Success' AND p.Status IN (N'Success', N'Succeeded'))
        OR (@StatusGroup = N'Failed' AND p.Status IN (N'Failed', N'Declined', N'Expired', N'Revoked', N'Duplicated'))
      )
ORDER BY p.CreatedAt DESC;";

            using (var connection = SqlHelper.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Param("@FromDate", fromDate.HasValue ? (object)fromDate.Value.Date : null, SqlDbType.Date));
                command.Parameters.Add(SqlHelper.Param("@ToDate", toDate.HasValue ? (object)toDate.Value.Date : null, SqlDbType.Date));
                command.Parameters.Add(SqlHelper.Param("@Pincode", string.IsNullOrWhiteSpace(pincode) ? (object)null : pincode.Trim(), SqlDbType.NVarChar));
                command.Parameters.Add(SqlHelper.Param("@SchoolId", schoolId.HasValue ? (object)schoolId.Value : null, SqlDbType.Int));
                command.Parameters.Add(SqlHelper.Param("@StatusGroup", string.IsNullOrWhiteSpace(statusGroup) ? (object)null : statusGroup, SqlDbType.NVarChar, 20));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(MapSearch(reader));
                    }
                }
            }

            return rows;
        }

        private static PaymentRecord MapSearch(SqlDataReader reader)
        {
            var record = new PaymentRecord
            {
                Id = reader.GetInt32(0),
                OrderId = reader.GetString(1).Trim(),
                SchoolId = reader.GetInt32(2),
                StudentId = reader.GetInt32(3),
                SchoolName = reader.GetString(4),
                StudentName = reader.GetString(5),
                Pincode = reader.IsDBNull(6) ? null : reader.GetString(6),
                PaymentType = reader.GetString(7),
                Amount = reader.GetDecimal(8),
                Currency = reader.GetString(9).Trim(),
                PayerName = reader.IsDBNull(10) ? null : reader.GetString(10),
                PayerEmail = reader.IsDBNull(11) ? null : reader.GetString(11),
                PayerPhone = reader.IsDBNull(12) ? null : reader.GetString(12),
                AlqasehPaymentId = reader.IsDBNull(13) ? null : reader.GetString(13),
                PaymentToken = reader.IsDBNull(14) ? null : reader.GetString(14),
                Status = reader.GetString(15),
                GatewayStatus = reader.IsDBNull(16) ? null : reader.GetString(16),
                ApprovalCode = reader.IsDBNull(17) ? null : reader.GetString(17),
                Rrn = reader.IsDBNull(18) ? null : reader.GetString(18),
                CreatedAt = reader.GetDateTime(19)
            };
            record.StatusDisplay = PaymentStatusMapper.ToArabic(record.Status);
            return record;
        }

        private static PaymentRecord Map(SqlDataReader reader)
        {
            return new PaymentRecord
            {
                Id = reader.GetInt32(0),
                OrderId = reader.GetString(1).Trim(),
                SchoolId = reader.GetInt32(2),
                StudentId = reader.GetInt32(3),
                SchoolName = reader.GetString(4),
                StudentName = reader.GetString(5),
                PaymentType = reader.GetString(6),
                Amount = reader.GetDecimal(7),
                Currency = reader.GetString(8).Trim(),
                PayerName = reader.IsDBNull(9) ? null : reader.GetString(9),
                PayerEmail = reader.IsDBNull(10) ? null : reader.GetString(10),
                PayerPhone = reader.IsDBNull(11) ? null : reader.GetString(11),
                AlqasehPaymentId = reader.IsDBNull(12) ? null : reader.GetString(12),
                PaymentToken = reader.IsDBNull(13) ? null : reader.GetString(13),
                Status = reader.GetString(14),
                GatewayStatus = reader.IsDBNull(15) ? null : reader.GetString(15),
                ApprovalCode = reader.IsDBNull(16) ? null : reader.GetString(16),
                Rrn = reader.IsDBNull(17) ? null : reader.GetString(17),
                CreatedAt = reader.GetDateTime(18)
            };
        }

        private static void AddCommonParameters(SqlCommand command, PaymentRecord payment)
        {
            command.Parameters.Add(SqlHelper.Param("@OrderId", payment.OrderId, SqlDbType.Char, 32));
            command.Parameters.Add(SqlHelper.Param("@SchoolId", payment.SchoolId, SqlDbType.Int));
            command.Parameters.Add(SqlHelper.Param("@StudentId", payment.StudentId, SqlDbType.Int));
            command.Parameters.Add(SqlHelper.Param("@PaymentType", payment.PaymentType, SqlDbType.NVarChar));
            command.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = payment.Amount
            });
            command.Parameters.Add(SqlHelper.Param("@Currency", payment.Currency, SqlDbType.Char));
            command.Parameters.Add(SqlHelper.Param("@PayerName", payment.PayerName, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@PayerEmail", payment.PayerEmail, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@PayerPhone", payment.PayerPhone, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@AlqasehPaymentId", payment.AlqasehPaymentId, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@PaymentToken", payment.PaymentToken, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@Status", payment.Status, SqlDbType.NVarChar));
            command.Parameters.Add(SqlHelper.Param("@GatewayStatus", payment.GatewayStatus, SqlDbType.NVarChar));
        }
    }
}
