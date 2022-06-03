using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Account {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        [StringLength(9)]
        [ForeignKey("User")]
        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
