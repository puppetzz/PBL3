using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;
using PBL3.Service;
using System.Security.Claims;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IBlobService _blobService;
        private readonly IReceiptService _receiptService;
        private readonly IExcelService _excelService;
        private readonly ICommodityService _commodityService;
        private const string _containerName = "images";

        public CommodityController(ShopGuitarContext context
            , IBlobService blobService
            , IReceiptService receiptService
            , IExcelService excelService
            , ICommodityService commodityService) {
            _context = context;
            _blobService = blobService;
            _receiptService = receiptService;
            _excelService = excelService;
            _commodityService = commodityService;
        }

        [HttpGet("commodity-sold/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> CommoditySold(DateTime fromDate, DateTime toDate) {
            
            List<Commodity> commoditiesSold = await _commodityService.GetCommoditiesSold(fromDate, toDate);

            return Ok(commoditiesSold);
        }

        [HttpGet("commodity")]
        [Authorize]
        public async Task<IActionResult> GetAllCommodity() {
            var commodities = await _context.Commodities
                .Select(c => new {
                    c.CommodityId,
                    c.Type,
                    c.Quantity,
                    c.Brand,
                    c.Name,
                    c.Price,
                    c.warrantyTime,
                    ImageUri = c.ImageName != null ? _blobService.GetBlob(c.ImageName, _containerName) : null
                }).ToListAsync();
            if (commodities != null)
                return Ok(commodities);

            return BadRequest("Commodities don't exists!");
        }

        [HttpGet("commodity/{id}")]
        [Authorize]
        public async Task<ActionResult> GetCommodity(string id) {
            var commodity = await _context.Commodities
                .Select(c => new {
                    c.CommodityId,
                    c.Type,
                    c.Quantity,
                    c.Brand,
                    c.Name,
                    c.Price,
                    c.warrantyTime,
                    ImageUri = c.ImageName == null ? null : _blobService.GetBlob(c.ImageName, _containerName)
                }).FirstOrDefaultAsync(c => c.CommodityId == id);
            if (commodity == null) {
                return BadRequest("Commodity does not exist!");
            }

            return Ok(commodity);
        }

        [HttpPost("add-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommodity([FromForm] AddCommodityDto commodity) {
            var addCommodity = await addCommodityAsync(commodity);

            ReceiptDto receiptDto = new ReceiptDto {
                CustomerName = commodity.EnterpriseName,
                CustomerPhoneNumber = commodity.EnterprisePhoneNumber,
                CustomerAddress = commodity.EnterpriseAddress
            };

            receiptDto.Commodity = new List<Tuple<string, int>>();
            receiptDto.Commodity.Add(new Tuple<string, int>(commodity.CommodityId, commodity.Quantity));

            bool isAddReceiptSuccessful = false;
            
            List<AddCommodityDto> addCommodityDtos = new List<AddCommodityDto>();
            addCommodityDtos.Add(commodity);

            try {
                isAddReceiptSuccessful = await _receiptService
                    .AddReceiptIn(receiptDto, getCurrentEmployeeId(), addCommodityDtos);
            } catch(Exception e) {
                return BadRequest(e.Message);
            }


            if (addCommodity && isAddReceiptSuccessful) {
                await _context.SaveChangesAsync();
                return Ok("Added!");
            }


            return BadRequest("Failed!");
        }

        [HttpPost("add-list-commodity-with-image")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommodities([FromForm]List<AddCommodityDto> commodityDtos) {
            List<Tuple<string, int>> commodities = new List<Tuple<string, int>>();
            List<AddCommodityDto> list = new List<AddCommodityDto>();
            foreach (AddCommodityDto commodity in commodityDtos) {
                var addCommodity = await addCommodityAsync(commodity);

                list.Add(commodity);

                commodities.Add(new Tuple<string, int>(commodity.CommodityId, commodity.Quantity));

                if (!addCommodity)
                    return BadRequest("Add Failed!");
            }

            ReceiptDto receiptDto = new ReceiptDto {
                CustomerName = commodityDtos[0].EnterpriseName,
                CustomerPhoneNumber = commodityDtos[0].EnterprisePhoneNumber,
                CustomerAddress = commodityDtos[0].EnterpriseAddress
            };

            receiptDto.Commodity = new List<Tuple<string, int>>();
            receiptDto.Commodity.AddRange(commodities);

            bool isAddReceiptSuccessful = false;

            try {
                isAddReceiptSuccessful = await _receiptService
                    .AddReceiptIn(receiptDto, getCurrentEmployeeId(), list);
            } catch (Exception e) {
                return BadRequest(e.Message);
            }

            if (!isAddReceiptSuccessful)
                return BadRequest("Add Failed!");

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPost("add-list-commodity-without-image")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommoditiesWithoutImage(List<CommodityWithoutImageDto> commodityDtos) {
            List<Tuple<string, int>> commodities = new List<Tuple<string, int>>();
            List<CommodityWithoutImageDto> list = new List<CommodityWithoutImageDto>();
            foreach (CommodityWithoutImageDto commodity in commodityDtos) {
                var addCommodity = await addCommodityWithoutImageAsync(commodity);

                list.Add(commodity);

                commodities.Add(new Tuple<string, int>(commodity.CommodityId, commodity.Quantity));

                if (!addCommodity)
                    return BadRequest("Add Failed!");
            }

            ReceiptDto receiptDto = new ReceiptDto {
                CustomerName = commodityDtos[0].EnterpriseName,
                CustomerPhoneNumber = commodityDtos[0].EnterprisePhoneNumber,
                CustomerAddress = commodityDtos[0].EnterpriseAddress
            };

            receiptDto.Commodity = new List<Tuple<string, int>>();
            receiptDto.Commodity.AddRange(commodities);

            bool isAddReceiptSuccessful = false;

            try {
                isAddReceiptSuccessful = await _receiptService
                    .AddReceiptInWithoutImage(receiptDto, getCurrentEmployeeId(), list);
            } catch (Exception e) {
                return BadRequest(e.Message);
            }

            if (!isAddReceiptSuccessful)
                return BadRequest("Add Failed!");

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPost("add-commodity-from-excel-file")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommoditiesFromExcelFile(IFormFile file) {
            List<CommodityWithoutImageDto> commodityDtos = new List<CommodityWithoutImageDto>();

            try {
                commodityDtos = await _excelService.GetCommodityFormExcelAsync(file);
            } catch (Exception e) {
                return BadRequest(e);
            }

            List<Tuple<string, int>> commodities = new List<Tuple<string, int>>();
            List<CommodityWithoutImageDto> list = new List<CommodityWithoutImageDto>();
            foreach (CommodityWithoutImageDto commodity in commodityDtos) {
                var addCommodity = await addCommodityWithoutImageAsync(commodity);

                list.Add(commodity);

                commodities.Add(new Tuple<string, int>(commodity.CommodityId, commodity.Quantity));

                if (!addCommodity)
                    return BadRequest("Add Failed!");
            }

            ReceiptDto receiptDto = new ReceiptDto {
                CustomerName = commodityDtos[0].EnterpriseName,
                CustomerPhoneNumber = commodityDtos[0].EnterprisePhoneNumber,
                CustomerAddress = commodityDtos[0].EnterpriseAddress
            };

            receiptDto.Commodity = new List<Tuple<string, int>>();
            receiptDto.Commodity.AddRange(commodities);

            bool isAddReceiptSuccessful = false;

            try {
                isAddReceiptSuccessful = await _receiptService
                    .AddReceiptInWithoutImage(receiptDto, getCurrentEmployeeId(), list);
            } catch (Exception e) {
                return BadRequest(e.Message);
            }

            if (!isAddReceiptSuccessful)
                return BadRequest("Add Failed!");

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPut("update-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> UpdateCommodity([FromForm]CommodityDto commodityDto) {
            var commodity = await _context.Commodities.FindAsync(commodityDto.CommodityId);

            if (commodity == null)
                return BadRequest("Commodity doesn't exists!");

            bool res = false;

            if (commodityDto.ImageFile != null) {
                _ = await _blobService.DeleteBlobAsync(commodity.ImageName, _containerName);

                res = await SaveImage(commodityDto.CommodityId, commodityDto.ImageFile);

                if (!res) {
                    return BadRequest("Image upload failed");
                }
            }

            commodity.Type = commodityDto.Type;
            commodity.Quantity = commodityDto.Quantity;
            commodity.Brand = commodityDto.Brand;
            commodity.Name = commodityDto.Name;
            commodity.Price = commodityDto.Price;
            commodity.warrantyTime = commodityDto.warrantyTime;

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Update successful",
                ImageAdded = res
            });
        }

        [HttpDelete("delete-commodity/{id}")]
        [Authorize(Roles ="admin")]
        public async Task<ActionResult> DeleteCommodity(string id) {
            var commodity = _context.Commodities.FirstOrDefault(c => c.CommodityId == id);

            if (commodity == null)
                return BadRequest("Commodity doesn't exists!");

            _ = await _blobService.DeleteBlobAsync(commodity.ImageName, _containerName);

            _context.Commodities.Remove(commodity);
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }

        [HttpDelete("delete-list-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteCommodity(List<string> ids) { 
            foreach (string id in ids) {
                var commodity = await _context.Commodities.FirstOrDefaultAsync(c => c.CommodityId == id);

                if (commodity == null)
                    return BadRequest($"Commodity with id: {id} does't exists!");

                _ = await _blobService.DeleteBlobAsync(commodity.ImageName, _containerName);

                _context.Commodities.Remove(commodity);
            }
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }

        [HttpGet("export-commodities-sold-to-excel/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> ExportCommoditesSold(DateTime fromDate, DateTime toDate) {
            List<Commodity> commoditiesSold = await _commodityService.GetCommoditiesSold(fromDate, toDate);

            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Commodities-Sold");
            Sheet.Cells["A1"].Value = "Commodity ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Quantity";
            Sheet.Cells["D1"].Value = "Brand";
            Sheet.Cells["E1"].Value = "Name";
            Sheet.Cells["F1"].Value = "Price";
            Sheet.Cells["G1"].Value = "Warranty Time";

            int row = 2;
            foreach (var item in commoditiesSold) {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.CommodityId;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Type;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Quantity;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.Brand;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Name;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Price;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.warrantyTime;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileContents = Ep.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0) {
                return NotFound();
            }

            string excelName = $"Commodities-Sold-List-{DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff")}.xlsx";

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: excelName
            );
        }

        [HttpGet("export-commodities-to-excel")]
        [Authorize]
        public async Task<ActionResult> ExportCommodites() {
            List<Commodity> commoditiesSold = await _context.Commodities
                .ToListAsync();

            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Commodities");
            Sheet.Cells["A1"].Value = "Commodity ID";
            Sheet.Cells["B1"].Value = "Type";
            Sheet.Cells["C1"].Value = "Quantity";
            Sheet.Cells["D1"].Value = "Brand";
            Sheet.Cells["E1"].Value = "Name";
            Sheet.Cells["F1"].Value = "Price";
            Sheet.Cells["G1"].Value = "Warranty Time";

            int row = 2;
            foreach (var item in commoditiesSold) {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.CommodityId;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Type;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Quantity;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.Brand;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Name;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Price;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.warrantyTime;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileContents = Ep.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0) {
                return NotFound();
            }

            string excelName = $"Commodities-List-{DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff")}.xlsx";

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: excelName
            );
        }

        private async Task<bool> addCommodityAsync(AddCommodityDto addCommodityDto) {
            if (!_context.Commodities.Any(c => c.CommodityId == addCommodityDto.CommodityId)) {
                string? fileName = null;

                if (addCommodityDto.ImageFile != null && addCommodityDto.ImageFile.Length > 1) {
                    fileName = Guid.NewGuid() + Path.GetExtension(addCommodityDto.ImageFile.FileName);

                    var res = await _blobService.UploadBlobAsync(fileName, addCommodityDto.ImageFile, _containerName);
                }

                Commodity newCommodity = new Commodity {
                    CommodityId = addCommodityDto.CommodityId,
                    Type = addCommodityDto.Type,
                    Quantity = addCommodityDto.Quantity,
                    Brand = addCommodityDto.Brand,
                    Name = addCommodityDto.Name,
                    Price = addCommodityDto.Price,
                    warrantyTime = addCommodityDto.warrantyTime,
                    ImageName = fileName
                };

                await _context.Commodities.AddAsync(newCommodity);
                await _context.SaveChangesAsync();
                return true;
            }

            var commodityDto = await _context.Commodities.FindAsync(addCommodityDto.CommodityId);
            commodityDto.Quantity += commodityDto.Quantity;

            return true;
        }

        private async Task<bool> addCommodityWithoutImageAsync(CommodityWithoutImageDto addCommodityDto) {
            if (!_context.Commodities.Any(c => c.CommodityId == addCommodityDto.CommodityId)) {
                Commodity newCommodity = new Commodity {
                    CommodityId = addCommodityDto.CommodityId,
                    Type = addCommodityDto.Type,
                    Quantity = addCommodityDto.Quantity,
                    Brand = addCommodityDto.Brand,
                    Name = addCommodityDto.Name,
                    Price = addCommodityDto.Price,
                    warrantyTime = addCommodityDto.warrantyTime,
                };

                await _context.Commodities.AddAsync(newCommodity);
                await _context.SaveChangesAsync();
                return true;
            }

            var commodityDto = await _context.Commodities.FindAsync(addCommodityDto.CommodityId);
            commodityDto.Quantity += commodityDto.Quantity;

            return true;
        }

        private async Task<bool> SaveImage(string commodityId, IFormFile file) {
            var commodity = _context.Commodities.Find(commodityId);

            if (file == null || file.Length < 1)
                return false;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var res = await _blobService.UploadBlobAsync(fileName, file, _containerName);

            if (res) {
                commodity.ImageName = fileName;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private string getCurrentEmployeeId() {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userClaims = identity.Claims;

            return userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
