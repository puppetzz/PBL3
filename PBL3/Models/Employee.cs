using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Employee {
        [Key]
        [StringLength(9)]
        [ForeignKey("User")]
        public string Id { get; set; }
        [Required]
        [StringLength(9)]
        public string? ManagerId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        [JsonIgnore]
        [InverseProperty("Employee")]
        public virtual Salaries Salaries { get; set; }
        [JsonIgnore]
        public virtual Employee? Manager { get; set; }
        [JsonIgnore]
        public virtual Titles Titles { get; set; }
        [JsonIgnore]
        public virtual ICollection<Receipt>? Receipts { get; set; }
    }
}
