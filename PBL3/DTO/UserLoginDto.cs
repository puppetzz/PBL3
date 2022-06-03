using System.ComponentModel.DataAnnotations;

namespace PBL3.DTO {
    public class UserLoginDto {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
