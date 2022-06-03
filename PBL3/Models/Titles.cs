using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Titles {
        [Key]
        [StringLength(9)]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }
    }
}
