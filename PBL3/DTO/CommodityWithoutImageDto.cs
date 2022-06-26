namespace PBL3.DTO {
    public class CommodityWithoutImageDto {
        public string CommodityId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string warrantyTime { get; set; } = string.Empty;

        public string EnterpriseName { get; set; } = string.Empty;
        public string EnterprisePhoneNumber { get; set; } = string.Empty;
        public string EnterpriseAddress { get; set; } = string.Empty;
    }
}
