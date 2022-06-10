namespace PBL3.DTO {
    public class ReceiptDto {
        public List<Tuple<string, int>> Commodity { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerAddress { get; set; }
    }
}
