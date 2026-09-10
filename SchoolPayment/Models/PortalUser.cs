namespace SchoolPayment.Models
{
    public class PortalUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}
