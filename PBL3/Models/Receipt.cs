using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Receipt {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ReceiptId { get; set; }
        [StringLength(9)]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }
        [JsonIgnore]
        public virtual ICollection<ReceiptCommodity> ReceiptCommodities { get; set; }
    }
}
