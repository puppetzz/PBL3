using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;
using PBL3.Service;
using System.Security.Claims;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IBlobService _blobService;
        private readonly IReceiptService _receiptService;
        private const string _containerName = "images";

        public ReceiptController(ShopGuitarContext context, IBlobService blobService, IReceiptService receiptService) {
            _context = context;
            _blobService = blobService;
            _receiptService = receiptService;
        }

        [HttpGet("receipt-sales")]
        [Authorize]
        public async Task<ActionResult> GetAllReceiptSales() {
            var receipts = await _context.Receipts
                .Where(r => r.IsSales)
                .Include(r => r.ReceiptCommodities)
                .ThenInclude(r => r.Commodity)
                .Select(r => new {
                    r.ReceiptId,
                    r.EmployeeId,
                    customer = _context.Contacts
                    .Where(c => c.ContactId == r.Contact.ContactId)
                    .Select(c => new {
                        c.Name,
                        c.PhoneNumber,
                        c.Address
                    }).FirstOrDefault(),
                    commodity = _context.ReceiptCommodities
                        .Where(rc => rc.ReceiptId == r.ReceiptId)
                        .Select(rc => new {
                            rc.CommodityId,
                            rc.Commodity.Type,
                            Quantity = Convert.ToInt32(
                                _context.ReceiptCommodities
                                .Where(o => o.ReceiptId == r.ReceiptId && o.CommodityId == rc.CommodityId)
                                .Select(o => o.CommodityQuantity)
                                .FirstOrDefault()),
                            rc.Commodity.Brand,
                            rc.Commodity.Name,
                            rc.Commodity.Price,
                            rc.Commodity.warrantyTime,
                            ImageUri = rc.Commodity.ImageName != null ? _blobService.GetBlob(rc.Commodity.ImageName, _containerName) : null
                        }).ToList(),
                    r.TotalPrice,
                    r.Date
                }).ToListAsync();

            return Ok(receipts);
        }

        [HttpGet("receipt-add-commodity")]
        [Authorize]
        public async Task<ActionResult> GetAllReceiptComodityAdded() {
            var receipts = await _context.Receipts
                .Where(r => !r.IsSales)
                .Include(r => r.ReceiptCommodities)
                .ThenInclude(r => r.Commodity)
                .Select(r => new {
                    r.ReceiptId,
                    r.EmployeeId,
                    Enterprise = _context.Contacts
                    .Where(c => c.ContactId == r.Contact.ContactId)
                    .Select(c => new {
                        c.Name,
                        c.PhoneNumber,
                        c.Address
                    }).FirstOrDefault(),
                    commodity = _context.ReceiptCommodities
                        .Where(rc => rc.ReceiptId == r.ReceiptId)
                        .Select(rc => new {
                            rc.CommodityId,
                            rc.Commodity.Type,
                            Quantity = Convert.ToInt32(
                                _context.ReceiptCommodities
                                .Where(o => o.ReceiptId == r.ReceiptId && o.CommodityId == rc.CommodityId)
                                .Select(o => o.CommodityQuantity)
                                .FirstOrDefault()),
                            rc.Commodity.Brand,
                            rc.Commodity.Name,
                            rc.Commodity.Price,
                            rc.Commodity.warrantyTime,
                            ImageUri = rc.Commodity.ImageName != null ? _blobService.GetBlob(rc.Commodity.ImageName, _containerName) : null
                        }).ToList(),
                    r.TotalPrice,
                    r.Date
                }).ToListAsync();

            return Ok(receipts);
        }

        [HttpGet("receipt/{receiptId}")]
        [Authorize]
        public async Task<ActionResult> GeReceipt(string receiptId) {
            var receipt = await _context.Receipts
                .Include(r => r.ReceiptCommodities)
                .ThenInclude(r => r.Commodity)
                .Where(r => r.ReceiptId == receiptId)
                .Select(r => new {
                    r.ReceiptId,
                    r.EmployeeId,
                    customer = _context.Contacts
                    .Where(c => c.ContactId == r.Contact.ContactId)
                    .Select(c => new {
                        c.Name,
                        c.PhoneNumber,
                        c.Address
                    }).FirstOrDefault(),
                    commodity = _context.ReceiptCommodities
                        .Where(rc => rc.ReceiptId == r.ReceiptId)
                        .Select(rc => new {
                            rc.CommodityId,
                            rc.Commodity.Type,
                            Quantity = Convert.ToInt32(
                                _context.ReceiptCommodities
                                .Where(o => o.ReceiptId == r.ReceiptId && o.CommodityId == rc.CommodityId)
                                .Select(o => o.CommodityQuantity)
                                .FirstOrDefault()),
                            rc.Commodity.Brand,
                            rc.Commodity.Name,
                            rc.Commodity.Price,
                            rc.Commodity.warrantyTime,
                            ImageUri = rc.Commodity.ImageName != null ? _blobService.GetBlob(rc.Commodity.ImageName, _containerName) : null
                        }).ToList(),
                    r.TotalPrice,
                    r.Date
                }).FirstOrDefaultAsync();
            if (receipt == null)
                return BadRequest("Receipt doesn't exist!");

            return Ok(receipt);
        }

        [HttpPost]
        [Route("add-receipt")]
        [Authorize(Roles = "admin, employee")]
        public async Task<ActionResult> AddReceipt(ReceiptDto receiptDto) {
            bool isSuccessful = false;

            try {
                isSuccessful = await _receiptService.AddReceiptSales(receiptDto, getCurrentEmployeeId());
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }

            if (isSuccessful)
                return Ok("Added!");

            return BadRequest("Add failed!");
        }

        [HttpDelete("delete-receipt/{id}")]
        [Authorize(Roles ="admin")]
        public async Task<ActionResult> Delete(string id) {
            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt == null)
                return BadRequest("Receipt does not exists!");

            _context.Receipts.Remove(receipt);

            await _context.SaveChangesAsync();
            
            return Ok("Deleted!");
        }

        [HttpGet("sales/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> TotalPrice(DateTime fromDate, DateTime toDate) { 
            List<Receipt> receipt = await _context.Receipts
                .Where(r => r.Date > fromDate && r.Date < toDate && r.IsSales).ToListAsync();
            decimal totalPrice = 0;
            foreach (Receipt r in receipt) {
                totalPrice += r.TotalPrice;
            }
            return Ok(totalPrice);
        }

        [HttpGet("revenue/{date}")]
        [Authorize]
        public async Task<ActionResult> GetRevenueOfTheDay(DateTime date) {
            Decimal moneyIn = await _context.Receipts
                .Where(r => r.Date == date && r.IsSales)
                .SumAsync(r => r.TotalPrice);
            Decimal moneyOut = await _context.Receipts
                .Where(r => r.Date == date && !r.IsSales)
                .SumAsync(r => r.TotalPrice);
            return Ok(new {
                MoneyIn = moneyIn,
                MoneyOut = moneyOut
            });
        }

        [HttpGet("revenue/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> GetReveue(DateTime fromDate, DateTime toDate) {
            Decimal moneyIn = await _context.Receipts
                .Where(r => r.Date > fromDate && r.Date < toDate && r.IsSales)
                .SumAsync(r => r.TotalPrice);
            Decimal moneyOut = await _context.Receipts
                .Where(r => r.Date > fromDate && r.Date < toDate && !r.IsSales)
                .SumAsync(r => r.TotalPrice);

            return Ok(new {
                MoneyIn = moneyIn,
                MoneyOut = moneyOut
            });
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
