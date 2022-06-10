using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Customer {
        [Key]
        [StringLength(9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<Receipt> Receipts { get; set; }
    }
}
