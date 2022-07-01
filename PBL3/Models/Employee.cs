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
        public string TitleName { get; set; }
        public Decimal Salary { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        [JsonIgnore]
        public virtual Employee? Manager { get; set; }
        [JsonIgnore]
        public virtual ICollection<Receipt>? Receipts { get; set; }
    }
}
