using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.DTO;
using System.Security.Claims;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase {
        private readonly ShopGuitarContext _context;

        public ReceiptController(ShopGuitarContext context) {
            _context = context;
        }

        [HttpGet("receipt")]
        [Authorize]
        public async Task<ActionResult> GetAllReceipt() {
            var receipts = await _context
                .ReceiptCommodities
                .Include(r => r.Receipt)
                .Include(r => r.Commodity)
                .ToListAsync();

            return Ok(receipts);
        }

        [HttpGet("receipt/{receiptId}")]
        [Authorize]
        public async Task<ActionResult> GeReceipt(string receiptId) {
            var receipt = await _context.ReceiptCommodities
                .Include(r => r.Receipt)
                .Include(r => r.Commodity)
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId);
            if (receipt == null)
                return BadRequest("Receipt doesn't exist!");
            receipt.Commodity.Quantity = receipt.CommodityQuantity;

            return Ok(receipt);
        }

        [HttpPost("add-receipt")]
        [Authorize(Roles ="admin, employee")]
        public async Task<ActionResult> AddReceipt(ReceiptDto receiptDto) {
            List<ReceiptCommodity> receiptCommodities = new List<ReceiptCommodity>();

            decimal totalPrice = 0;

            string newId = await generationNewReceiptId();


            foreach (string commodityid in receiptDto.CommodityId) {
                var commodity = await _context.Commodities.FirstOrDefaultAsync(c => c.CommodityId == commodityid);
                if (commodity == null)
                    return BadRequest("Commodity does not exist!");
                totalPrice += commodity.Price * receiptDto.Quantity;
                receiptCommodities.Add(new ReceiptCommodity {
                    ReceiptId = newId,
                    CommodityId = commodityid,
                    CommodityQuantity = receiptDto.Quantity
                });
                commodity.Quantity -= receiptDto.Quantity;
            }


            Receipt receipt = new Receipt {
                ReceiptId = newId,
                EmployeeId = getCurrentEmployeeId(),
                TotalPrice = totalPrice,
                Date = receiptDto.Date,
            };

            await _context.Receipts.AddAsync(receipt);
            await _context.ReceiptCommodities.AddRangeAsync(receiptCommodities);

            await _context.SaveChangesAsync();
            return Ok("Added!");
        }

        [HttpDelete("delete-receipt/{id}")]
        [Authorize("admin")]
        public async Task<ActionResult> Delete(string id) {
            var receipt = await _context.Receipts.FirstOrDefaultAsync(r => r.ReceiptId == id);
            if (receipt == null)
                return BadRequest("Receipt does not exists!");

            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }

        [HttpGet("sales/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> TotalPrice(DateTime fromDate, DateTime toDate) { 
            List<Receipt> receipt = await _context.Receipts.Where(r => r.Date > fromDate && r.Date < toDate).ToListAsync();
            decimal totalPrice = 0;
            foreach (Receipt r in receipt) {
                totalPrice += r.TotalPrice;
            }
            return Ok(totalPrice);
        }

        private async Task<string> generationNewReceiptId() {
            var lastReceipt = await _context.Receipts.OrderByDescending(r => r.ReceiptId).FirstOrDefaultAsync();
            int newId = 0;
            if (lastReceipt != null) {
                string id = lastReceipt.ReceiptId;
                newId = Convert.ToInt32(id.Substring(id.Length - 3));
                if (newId < 999)
                    newId += 1;
                else
                    newId = 1;
            } else {
                newId = 1;
            }
            return $"RC{DateTime.Now.Day}" +
                $"{DateTime.Now.Month}" +
                $"{DateTime.Now.Year.ToString().Substring(2)}" +
                $"{newId:D3}";
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
