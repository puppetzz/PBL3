using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Contact {
        [Key]
        [StringLength(9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ContactId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        [JsonIgnore]
        public virtual ICollection<Receipt> Receipts { get; set; }
    }
}
