using PBL3.Models;

namespace PBL3.Service {
    public interface ICommodityService {
        Task<List<Commodity>> GetCommoditiesSold(DateTime fromDate, DateTime toDate);
    }
}
