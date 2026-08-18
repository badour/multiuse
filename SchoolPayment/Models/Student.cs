namespace SchoolPayment.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public int StageId { get; set; }
        public string FullName { get; set; }
        public string StudentNumber { get; set; }
        public string StageName { get; set; }
        public decimal TotalCost { get; set; }
        public decimal PaidCost { get; set; }
        public decimal RemainCost { get; set; }
        public decimal DebtCost { get; set; }
        public decimal DiscountCost { get; set; }
    }
}
