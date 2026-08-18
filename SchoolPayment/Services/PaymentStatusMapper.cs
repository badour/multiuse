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
    }
}
