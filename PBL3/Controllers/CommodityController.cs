using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;
using PBL3.Service;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IBlobService _blobService;
        private const string _containerName = "images";

        public CommodityController(ShopGuitarContext context, IBlobService blobService) {
            _context = context;
            _blobService = blobService;
        }

        [HttpGet("commodity-sold/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> CommoditySold(DateTime fromDate, DateTime toDate) {
            List<ReceiptCommodity> receiptCommodity = await _context.ReceiptCommodities
                .Where(r => r.Receipt.Date > fromDate && r.Receipt.Date < toDate)
                .ToListAsync();
            List<Commodity> commoditySold = new List<Commodity>();
            foreach (ReceiptCommodity receipt in receiptCommodity) {
                if (commoditySold.Any(c => c.CommodityId == receipt.CommodityId)) {
                    commoditySold.FirstOrDefault(c => c.CommodityId == receipt.CommodityId).Quantity += receipt.CommodityQuantity;
                } else {
                    Commodity commodity = await _context.Commodities.FirstOrDefaultAsync(c => c.CommodityId == receipt.CommodityId);
                    commodity.Quantity = receipt.CommodityQuantity;
                    commoditySold.Add(commodity);
                }
            }

            return Ok(commoditySold);
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
        public async Task<ActionResult> AddCommodity([FromForm]CommodityDto commodityDto) {
            var addCommodity = await addCommodityAsync(commodityDto);

            if (addCommodity) {
                await _context.SaveChangesAsync();
                return Ok("Added!");
            }

            return BadRequest("Failed!");
        }

        [HttpPost("add-list-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommodities(List<CommodityDto> commodityDtos) {
            foreach (CommodityDto commodityDto in commodityDtos) {
                var addCommodity = await addCommodityAsync(commodityDto);

                if (!addCommodity) {
                    return BadRequest("Failed!");
                }
            }

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

        private async Task<bool> addCommodityAsync(CommodityDto commodityDto) {
            if (!_context.Commodities.Any(c => c.CommodityId == commodityDto.CommodityId)) {
                string? fileName = null;

                if (commodityDto.ImageFile != null && commodityDto.ImageFile.Length > 1) {
                    fileName = Guid.NewGuid() + Path.GetExtension(commodityDto.ImageFile.FileName);

                    var res = await _blobService.UploadBlobAsync(fileName, commodityDto.ImageFile, _containerName);
                }

                Commodity newCommodity = new Commodity {
                    CommodityId = commodityDto.CommodityId,
                    Type = commodityDto.Type,
                    Quantity = commodityDto.Quantity,
                    Brand = commodityDto.Brand,
                    Name = commodityDto.Name,
                    Price = commodityDto.Price,
                    warrantyTime = commodityDto.warrantyTime,
                    ImageName = fileName
                };

                _context.Commodities.Add(newCommodity);
                return true;
            }

            var commodity = await _context.Commodities.FindAsync(commodityDto.CommodityId);
            commodity.Quantity += commodityDto.Quantity;

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
    }
}
