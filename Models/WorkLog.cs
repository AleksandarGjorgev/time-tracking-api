namespace TimeTrackingAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string EmploymentType { get; set; }
        public bool IsActive { get; set; }
    }
}
