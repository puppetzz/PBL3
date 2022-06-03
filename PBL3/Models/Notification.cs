using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PBL3.Models {
    public class Notification {
        [Key]
        public string NotificationId { get; set; }
        public string TitleName { get; set; }
        public string Content { get; set; }
        public DateTime DatePost { get; set; } = DateTime.Now;
        public string? ManagerIdUpdated { get; set; } = null;
        public DateTime? DateUpdate { get; set; } = null;
        [StringLength(9)]
        [ForeignKey("Manager")]
        public string ManagerId { get; set; }
        public virtual Employee Manager { get; set; }
    }
}
