namespace PBL3.DTO {
    public class NotificationDto {
        public string? NotificationId { get; set; } = null;
        public string TitleName { get; set; }
        public string Content { get; set; }
        public DateTime DatePost { get; set; } = DateTime.Now;
        public DateTime? DateUpdate { get; set; } = DateTime.Now;
    }
}
