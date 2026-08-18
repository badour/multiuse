namespace SchoolPayment.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public int StageId { get; set; }
        public string FullName { get; set; }
        public string StudentNumber { get; set; }
        public decimal OutstandingDebt { get; set; }
    }
}
