using System.ComponentModel.DataAnnotations;

namespace PBL3.DTO {
    public class ChangePasswordDto {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters!")]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
