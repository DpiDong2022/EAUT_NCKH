using System.ComponentModel.DataAnnotations;

namespace EAUT_NCKH.Web.DTOs {
    public class LoginViewModel {
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        [StringLength(100, ErrorMessage = "Chiều của tài khoản không được vượt quá 100 kí tự")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsSaveAccount { get; set; }
    }
}