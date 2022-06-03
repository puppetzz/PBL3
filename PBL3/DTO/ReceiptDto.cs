namespace PBL3.DTO {
    public class ReceiptDto {
        public List<Tuple<string, int>> Commodity { get; set; }
        public DateTime Date { get; set; }
    }
}
