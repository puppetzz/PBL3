namespace PBL3.DTO {
    public class EmployeeDto {
        public string EmployeeId { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TitleName { get; set; } = string.Empty;
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
        public decimal slary { get; set; }
    }
}
