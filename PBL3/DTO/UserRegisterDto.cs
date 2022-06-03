using System.ComponentModel.DataAnnotations;

namespace PBL3.DTO {
    public class UserRegisterDto {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
