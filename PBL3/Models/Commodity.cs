using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace PBL3.Models {
    public class Commodity {
        [Key]
        [StringLength(20)]
        public string CommodityId { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string warrantyTime { get; set; }
        public string? ImageName { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReceiptCommodity> ReceiptCommodities { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}

