using PBL3.DTO;
using PBL3.Models;

namespace PBL3.Service {
    public interface IExcelService {
        Task<List<AddEmployeeDto>> GetEmployeeFormExcelAsync(IFormFile file);
        Task<List<CommodityWithoutImageDto>> GetCommodityFormExcelAsync(IFormFile file);
    }
}
