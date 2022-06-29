using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;

namespace PBL3.Service {
    public class CommodityService : ICommodityService {
        private readonly ShopGuitarContext _context;

        public CommodityService(ShopGuitarContext context) {
            _context = context;
        }

        public async Task<List<Commodity>> GetCommoditiesSold(DateTime fromDate, DateTime toDate) {
            List<ReceiptCommodity> receiptCommodity = await _context.ReceiptCommodities
                .Where(r => r.Receipt.Date >= fromDate && r.Receipt.Date <= toDate && r.Receipt.IsSales)
                .ToListAsync();

            List<Commodity> commoditiesSold = new List<Commodity>();

            foreach (ReceiptCommodity receipt in receiptCommodity) {
                if (commoditiesSold.Any(c => c.CommodityId == receipt.CommodityId)) {
                    commoditiesSold
                        .FirstOrDefault(c => c.CommodityId == receipt.CommodityId).Quantity += receipt.CommodityQuantity;
                } else {
                    var commodity = await _context.Commodities
                        .FirstOrDefaultAsync(c => c.CommodityId == receipt.CommodityId);
                    if (commodity != null) {
                        commodity.Quantity = receipt.CommodityQuantity;
                        commoditiesSold.Add(commodity);
                    }
                }
            }

            return commoditiesSold;
        }
    }
}
