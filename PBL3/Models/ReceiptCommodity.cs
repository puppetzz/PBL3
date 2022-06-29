using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class ReceiptCommodity {
        [ForeignKey("Receipt")]
        public string ReceiptId { get; set; }
        [ForeignKey("Commodity")]
        [StringLength(20)]
        public string CommodityId { get; set; }
        public int CommodityQuantity { get; set; }

        [JsonIgnore]
        public virtual Receipt Receipt { get; set; }
        [JsonIgnore]
        public virtual Commodity Commodity { get; set; }
    }
}
