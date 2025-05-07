using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class AccountDto {
        public int Id { get; set; }

        public string Fullname { get; set; } = null!;

        public bool Sex { get; set; }

        public string Email { get; set; } = null!;

        public string Phonenumber { get; set; } = null!;

        public string? Avatar { get; set; }

        public string Password { get; set; } = null!;

        public Major Major { get; set; }

        public int? Departmentid { get; set; }

        public int Roleid { get; set; }

        public virtual Department? Department { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

        public AccountDto(Account account) {
            Id = account.Id;
            Fullname = account.Fullname;
            Email = account.Email;
            Phonenumber = account.Phonenumber;
            Departmentid = account.Departmentid;
            Teachers = account.Teachers;
            Department = account.Department;
            if (Teachers.Count > 0) {
                Major = account.Teachers.First().Major;
            }
        }

        public AccountDto() {
        }
    }
}
