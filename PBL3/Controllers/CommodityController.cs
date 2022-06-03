using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase {
        private readonly ShopGuitarContext _context;

        public CommodityController(ShopGuitarContext context) {
            _context = context;
        }

        [HttpGet("commodity-sold/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> CommoditySold(DateTime fromDate, DateTime toDate) {
            List<ReceiptCommodity> receiptCommodity = await _context.ReceiptCommodities
                .Where(r => r.Receipt.Date > fromDate && r.Receipt.Date <toDate)
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
            var commodities = await _context.Commodities.ToListAsync();
            if (commodities != null)
                return Ok(commodities);

            return BadRequest("Commodities don't exists!");
        }

        [HttpGet("commodity/{id}")]
        [Authorize]
        public async Task<ActionResult> GetCommodity(string id) { 
            var commodity = await _context.Commodities.FirstOrDefaultAsync(c => c.CommodityId == id);
            if (commodity == null) {
                return BadRequest("Commodity does not exists!");
            }

            return Ok(commodity);
        }

        [HttpPost("add-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommodity(CommodityDto commodityDto) {
            await addCommodityAsync(commodityDto);

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPost("add-list-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCommodities(List<CommodityDto> commodityDtos) {
            foreach (CommodityDto commodity in commodityDtos) {
                await addCommodityAsync(commodity);
            }

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPut("update-commodity")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> UpdateCommodity(CommodityDto commodityDto) {
            var commodity = await _context.Commodities.FindAsync(commodityDto.CommodityId);

            if (commodity == null)
                return BadRequest("Commodity doesn't exists!");

            commodity.Type = commodityDto.Type;
            commodity.Quantity = commodityDto.Quantity;
            commodity.Brand = commodityDto.Brand;
            commodity.Name = commodityDto.Name;
            commodity.Price = commodityDto.Price;
            commodity.warrantyTime = commodityDto.warrantyTime;

            await _context.SaveChangesAsync();

            return Ok("Updated!");
        }

        [HttpDelete("delete-commodity/{id}")]
        [Authorize(Roles ="admin")]
        public async Task<ActionResult> DeleteCommodity(string id) {
            var commodity = _context.Commodities.FirstOrDefault(c => c.CommodityId == id);

            if (commodity == null)
                return BadRequest("Commodity doesn't exists!");
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
                _context.Commodities.Remove(commodity);
            }
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }

        private async Task addCommodityAsync(CommodityDto commodityDto) {
            if (!_context.Commodities.Any(c => c.CommodityId == commodityDto.CommodityId)) {
                Commodity newCommodity = new Commodity {
                    CommodityId = commodityDto.CommodityId,
                    Type = commodityDto.Type,
                    Quantity = commodityDto.Quantity,
                    Brand = commodityDto.Brand,
                    Name = commodityDto.Name,
                    Price = commodityDto.Price,
                    warrantyTime = commodityDto.warrantyTime
                };
                _context.Commodities.Add(newCommodity);
            } else {
                var commodity = await _context.Commodities.FindAsync(commodityDto.CommodityId);
                commodity.Quantity += commodityDto.Quantity;
            }
        }
    }
}
