using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EAUT_NCKH.Web.DTOs {
    public class SinglePropertyViewModel {
    }

    public class Email {
        [AllowNull]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string EmailAddress { get; set; }
    }
}
