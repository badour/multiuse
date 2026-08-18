using System;

namespace SchoolPayment.Models
{
    public static class PaymentTypes
    {
        public const string CurrentYearInstallment = "اقساط عام حالي";
        public const string Debt = "ديون";
    }

    public class PaymentRecord
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int SchoolId { get; set; }
        public int StageId { get; set; }
        public int StudentId { get; set; }
        public string SchoolName { get; set; }
        public string StageName { get; set; }
        public string StudentName { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PayerName { get; set; }
        public string PayerEmail { get; set; }
        public string PayerPhone { get; set; }
        public string AlqasehPaymentId { get; set; }
        public string PaymentToken { get; set; }
        public string Status { get; set; }
        public string GatewayStatus { get; set; }
        public string ApprovalCode { get; set; }
        public string Rrn { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
