using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Salaries {
        [Key]
        [StringLength(9)]
        public string Id { get; set; }
        [Column(TypeName = "decimal(15, 2)")]
        public decimal Salary { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        [JsonIgnore]
        [ForeignKey("Id")]
        public virtual Employee Employee { get; set; }
    }
}
