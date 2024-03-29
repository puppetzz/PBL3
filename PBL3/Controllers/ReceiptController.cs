﻿using Microsoft.AspNetCore.Authorization;
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
                }).OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.ReceiptId)
                .ToListAsync();

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
                }).OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.ReceiptId)
                .ToListAsync();

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
        [Authorize(Roles = "admin, 0, employee")]
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
        [Authorize(Roles ="admin, 0")]
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
            List<RevenueDto> revenues = new List<RevenueDto>();
            for (DateTime d = fromDate; d <= toDate; d = d.AddDays(1)) {
                Decimal moneyIn = await _context.Receipts
                .Where(r => r.Date == d && r.IsSales)
                .SumAsync(r => r.TotalPrice);
                Decimal moneyOut = await _context.Receipts
                    .Where(r => r.Date == d && !r.IsSales)
                    .SumAsync(r => r.TotalPrice);
                revenues.Add(new RevenueDto {
                    Date = d,
                    MoneyIn = moneyIn,
                    MoneyOut = moneyOut
                });
            }

            return Ok(revenues);
        }

        [HttpGet("export-revenue/{fromDate}/{toDate}")]
        [Authorize]
        public async Task<ActionResult> ExportRevenue(DateTime fromDate, DateTime toDate) {
            List<RevenueDto> revenues = new List<RevenueDto>();
            for (DateTime d = fromDate; d <= toDate; d = d.AddDays(1)) {
                Decimal moneyIn = await _context.Receipts
                .Where(r => r.Date == d && r.IsSales)
                .SumAsync(r => r.TotalPrice);
                Decimal moneyOut = await _context.Receipts
                    .Where(r => r.Date == d && !r.IsSales)
                    .SumAsync(r => r.TotalPrice);
                revenues.Add(new RevenueDto {
                    Date = d,
                    MoneyIn = moneyIn,
                    MoneyOut = moneyOut
                });
            }

            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Employee");
            Sheet.Cells["A1"].Value = "Date";
            Sheet.Cells["B1"].Value = "Money In";
            Sheet.Cells["C1"].Value = "Money Out";

            int row = 2;
            foreach (var item in revenues) {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.Date.ToString("dd-MM-yyyy");
                Sheet.Cells[string.Format("B{0}", row)].Value = item.MoneyIn;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.MoneyOut;
                row++;
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileContents = Ep.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0) {
                return NotFound();
            }

            string excelName = $"Revenues-List-{DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff")}.xlsx";

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: excelName
            );
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
