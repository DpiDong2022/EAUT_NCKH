namespace EAUT_NCKH.Web.Models {
    public class User {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string PasswordHash { get; set; } // Store hashed password
        public string Role { get; set; } // e.g., "Admin", "User"

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndTime { get; set; }
    }
}
