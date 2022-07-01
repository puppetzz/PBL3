using PBL3.DTO;
using PBL3.Models;

namespace PBL3.Service {
    public interface IExcelService {
        Task<List<AddEmployeeDto>> GetEmployeeFromExcelAsync(IFormFile file);
        Task<List<CommodityWithoutImageDto>> GetCommodityFromExcelAsync(IFormFile file);
    }
}
