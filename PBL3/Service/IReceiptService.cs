using PBL3.DTO;

namespace PBL3.Service {
    public interface IReceiptService {
        Task<bool> AddReceiptSales(ReceiptDto receiptDto, string currentId);
        Task<bool> AddReceiptIn(ReceiptDto receiptDto, string currentId, List<AddCommodityDto> addCommodityDtos);
        Task<bool> AddReceiptInWithoutImage(ReceiptDto receiptDto
            , string currentId
            , List<CommodityWithoutImageDto> addCommodityDtos);
        Task<string> generationNewCustomerId();
        Task<string> generationNewReceiptId();
    }
}
