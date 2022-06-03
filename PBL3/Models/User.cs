using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class User {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(9)]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public virtual Account Account { get; set; }
        [JsonIgnore]
        public virtual Employee Employee { get; set; }
    }
}
