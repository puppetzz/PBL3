using OfficeOpenXml;
using PBL3.DTO;
using PBL3.Models;

namespace PBL3.Service {
    public class ExcelService : IExcelService {
        public async Task<List<CommodityWithoutImageDto>> GetCommodityFormExcelAsync(IFormFile file) {
            var list = new List<CommodityWithoutImageDto>();
            using (var stream = new MemoryStream()) {
                await file.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream)) {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowcount = worksheet.Dimension.Rows;

                    if (rowcount == 0)
                        return null;

                    for (int row = 2; row <= rowcount; row++) {
                        for (int col = 1; col <= 10; col++) {
                            if (worksheet.Cells[row, col].Value == null)
                                throw new NullReferenceException("Data from excel file has null value");
                        }
                        list.Add(new CommodityWithoutImageDto {
                            CommodityId = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            Type = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            Quantity = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                            Brand = worksheet.Cells[row, 4].Value.ToString().Trim(),
                            Name = worksheet.Cells[row, 5].Value.ToString().Trim(),
                            Price = Convert.ToDecimal(worksheet.Cells[row, 6].Value),
                            warrantyTime = worksheet.Cells[row, 7].Value.ToString().Trim(),
                            EnterpriseName = worksheet.Cells[row, 8].Value.ToString().Trim(),
                            EnterprisePhoneNumber = worksheet.Cells[row, 9].Value.ToString().Trim(),
                            EnterpriseAddress = worksheet.Cells[row, 10].Value.ToString().Trim(),
                        });
                    }
                }
            }
            return list;
        }
        public async Task<List<AddEmployeeDto>?> GetEmployeeFormExcelAsync(IFormFile file) {
            var list = new List<AddEmployeeDto>();
            using (var stream = new MemoryStream()) {
                await file.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream)) {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowcount = worksheet.Dimension.Rows;

                    if (rowcount == 0)
                        return null;

                    for (int row = 2; row <= rowcount; row++) {
                        for (int col = 1; col <= 13; col++) {
                            if (worksheet.Cells[row, col].Value == null)
                                throw new NullReferenceException("Data from excel file has null value");
                        }
                        list.Add(new AddEmployeeDto {
                            ManagerId = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            FirstName = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            LastName = worksheet.Cells[row, 3].Value.ToString().Trim(),
                            Gender = Convert.ToBoolean(worksheet.Cells[row, 4].Value),
                            DateOfBirth = Convert.ToDateTime(worksheet.Cells[row, 5].Value),
                            PhoneNumber = worksheet.Cells[row, 6].Value.ToString().Trim(),
                            Email = worksheet.Cells[row, 7].Value.ToString().Trim(),
                            Address = worksheet.Cells[row, 8].Value.ToString().Trim(),
                            Role = worksheet.Cells[row, 9].Value.ToString().Trim(),
                            TitleName = worksheet.Cells[row, 10].Value.ToString().Trim(),
                            DateIn = Convert.ToDateTime(worksheet.Cells[row, 11].Value),
                            DateOut = Convert.ToDateTime(worksheet.Cells[row, 12].Value),
                            salary = Convert.ToDecimal(worksheet.Cells[row, 13].Value),
                        });
                    }
                }
            }
            return list;
        }
        
    }
}
