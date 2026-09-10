using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SchoolPayment.Data
{
    public static class SqlHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SchoolPaymentDb"].ConnectionString; }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlParameter Param(string name, object value, SqlDbType type, int size = 0)
        {
            var parameter = new SqlParameter(name, type)
            {
                Value = value ?? (object)DBNull.Value
            };

            if (size > 0)
            {
                parameter.Size = size;
            }
            else if (type == SqlDbType.Char || type == SqlDbType.NChar || type == SqlDbType.VarChar || type == SqlDbType.NVarChar)
            {
                var text = value as string;
                parameter.Size = text == null ? 1 : Math.Max(text.Length, 1);
            }

            return parameter;
        }
    }
}
