using System.ComponentModel.DataAnnotations;

namespace PBL3.DTO {
    public class ResetPasswordDto {
        [Required]
        public string UserId { get; set; }
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters!")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ComfirmPassword { get; set; } = string.Empty;
    }
}
