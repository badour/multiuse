using System;

namespace SchoolPayment.Services
{
    public static class PaymentStatusMapper
    {
        public static string ToLocalStatus(string gatewayStatus)
        {
            if (string.IsNullOrWhiteSpace(gatewayStatus))
            {
                return "Pending";
            }

            switch (gatewayStatus.Trim().ToLowerInvariant())
            {
                case "success":
                case "succeeded":
                    return "Success";
                case "failed":
                    return "Failed";
                case "declined":
                    return "Declined";
                case "expired":
                    return "Expired";
                case "revoked":
                    return "Revoked";
                case "duplicated":
                    return "Duplicated";
                case "prepared":
                    return "Prepared";
                case "retried":
                    return "Retried";
                case "pending":
                    return "Pending";
                default:
                    return gatewayStatus;
            }
        }

        public static bool IsFailed(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            switch (status.Trim().ToLowerInvariant())
            {
                case "failed":
                case "declined":
                case "expired":
                case "revoked":
                case "duplicated":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSuccess(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            switch (status.Trim().ToLowerInvariant())
            {
                case "success":
                case "succeeded":
                    return true;
                default:
                    return false;
            }
        }

        public static string ToArabic(string status)
        {
            if (IsSuccess(status))
            {
                return "نجاح";
            }

            if (IsFailed(status))
            {
                return "فشل";
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return "—";
            }

            switch (status.Trim().ToLowerInvariant())
            {
                case "pending":
                case "prepared":
                case "retried":
                    return "قيد المعالجة";
                default:
                    return status;
            }
        }
    }
}
