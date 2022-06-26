using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;

namespace PBL3.Service {
    public class ReceiptService : IReceiptService {
        private readonly ShopGuitarContext _context;

        public ReceiptService(ShopGuitarContext context) {
            _context = context;
        }

        public async Task<bool> AddReceiptIn(ReceiptDto receiptDto
            , string currentId
            , List<AddCommodityDto> addCommodityDtos) {
            List<ReceiptCommodity> receiptCommodities = new List<ReceiptCommodity>();

            decimal totalPrice = 0;

            string newId = await generationNewReceiptId();

            foreach (Tuple<string, int> commodity in receiptDto.Commodity) {
                var addCommodityDto = addCommodityDtos
                    .FirstOrDefault(c => c.CommodityId == commodity.Item1);

                if (addCommodityDto == null)
                    throw new NullReferenceException("Commodity does not exist!");

                if (addCommodityDto.Quantity <= 0)
                    throw new Exception("Sold out!");

                totalPrice += addCommodityDto.Price * commodity.Item2;

                receiptCommodities.Add(new ReceiptCommodity {
                    ReceiptId = newId,
                    CommodityId = commodity.Item1,
                    CommodityQuantity = commodity.Item2
                });
            }

            var customer = await _context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == receiptDto.CustomerPhoneNumber);

            string newEnterpriseId;

            if (customer == null) {
                newEnterpriseId = await generationNewCustomerId();
                await _context.Contacts.AddAsync(new Contact {
                    ContactId = newEnterpriseId,
                    Name = receiptDto.CustomerName,
                    PhoneNumber = receiptDto.CustomerPhoneNumber,
                    Address = receiptDto.CustomerAddress
                });
            } else
                newEnterpriseId = customer.ContactId;

            Receipt receipt = new Receipt {
                ReceiptId = newId,
                EmployeeId = currentId,
                TotalPrice = totalPrice,
                ContactId = newEnterpriseId,
                Date = DateTime.UtcNow.AddHours(7),
                IsSales = false
            };

            await _context.Receipts.AddAsync(receipt);
            await _context.ReceiptCommodities.AddRangeAsync(receiptCommodities);

            return true;
        }

        public async Task<bool> AddReceiptInWithoutImage(ReceiptDto receiptDto
            , string currentId
            , List<CommodityWithoutImageDto> addCommodityDtos) {
            List<ReceiptCommodity> receiptCommodities = new List<ReceiptCommodity>();

            decimal totalPrice = 0;

            string newId = await generationNewReceiptId();

            foreach (Tuple<string, int> commodity in receiptDto.Commodity) {
                var addCommodityDto = addCommodityDtos
                    .FirstOrDefault(c => c.CommodityId == commodity.Item1);

                if (addCommodityDto == null)
                    throw new NullReferenceException("Commodity does not exist!");

                if (addCommodityDto.Quantity <= 0)
                    throw new Exception("Sold out!");

                totalPrice += addCommodityDto.Price * commodity.Item2;

                receiptCommodities.Add(new ReceiptCommodity {
                    ReceiptId = newId,
                    CommodityId = commodity.Item1,
                    CommodityQuantity = commodity.Item2
                });
            }

            var customer = await _context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == receiptDto.CustomerPhoneNumber);

            string newEnterpriseId;

            if (customer == null) {
                newEnterpriseId = await generationNewCustomerId();
                await _context.Contacts.AddAsync(new Contact {
                    ContactId = newEnterpriseId,
                    Name = receiptDto.CustomerName,
                    PhoneNumber = receiptDto.CustomerPhoneNumber,
                    Address = receiptDto.CustomerAddress
                });
            } else
                newEnterpriseId = customer.ContactId;

            Receipt receipt = new Receipt {
                ReceiptId = newId,
                EmployeeId = currentId,
                TotalPrice = totalPrice,
                ContactId = newEnterpriseId,
                Date = DateTime.UtcNow.AddHours(7),
                IsSales = false
            };

            await _context.Receipts.AddAsync(receipt);
            await _context.ReceiptCommodities.AddRangeAsync(receiptCommodities);

            return true;
        }

        public async Task<bool> AddReceiptSales(ReceiptDto receiptDto, string currentId) {
            List<ReceiptCommodity> receiptCommodities = new List<ReceiptCommodity>();

            decimal totalPrice = 0;

            string newId = await generationNewReceiptId();

            foreach (Tuple<string, int> commodity in receiptDto.Commodity) {
                var commoditydb = await _context.Commodities
                    .FirstOrDefaultAsync(c => c.CommodityId == commodity.Item1);

                if (commoditydb == null)
                    throw new NullReferenceException("Commodity does not exist!");

                if (commoditydb.Quantity <= 0)
                    throw new Exception("Sold out!");

                totalPrice += commoditydb.Price * commodity.Item2;
                commoditydb.Quantity -= commodity.Item2;

                receiptCommodities.Add(new ReceiptCommodity {
                    ReceiptId = newId,
                    CommodityId = commodity.Item1,
                    CommodityQuantity = commodity.Item2
                });
            }

            var customer = await _context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == receiptDto.CustomerPhoneNumber);

            string newCustomerId;

            if (customer == null) {
                newCustomerId = await generationNewCustomerId();
                await _context.Contacts.AddAsync(new Contact {
                    ContactId = newCustomerId,
                    Name = receiptDto.CustomerName,
                    PhoneNumber = receiptDto.CustomerPhoneNumber,
                    Address = receiptDto.CustomerAddress
                });
            } else
                newCustomerId = customer.ContactId;

            Receipt receipt = new Receipt {
                ReceiptId = newId,
                EmployeeId = currentId,
                TotalPrice = totalPrice,
                ContactId = newCustomerId,
                Date = DateTime.UtcNow.AddHours(7),
                IsSales = true
            };

            await _context.Receipts.AddAsync(receipt);
            await _context.ReceiptCommodities.AddRangeAsync(receiptCommodities);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> generationNewCustomerId() {
            var lastReceipt = await _context.Receipts.OrderByDescending(r => r.ReceiptId).FirstOrDefaultAsync();
            int newId = 0;
            if (lastReceipt != null) {
                string id = lastReceipt.ReceiptId;
                newId = Convert.ToInt32(id.Substring(id.Length - 4));
                if (newId < 9999)
                    newId += 1;
                else
                    newId = 1;
            } else {
                newId = 1;
            }
            return $"C{DateTime.Now.Year.ToString().Substring(2)}" +
                $"{DateTime.Now.Month:D2}" +
                $"{newId:D4}";
        }

        public async Task<string> generationNewReceiptId() {
            var lastReceipt = await _context.Receipts.OrderByDescending(r => r.ReceiptId).FirstOrDefaultAsync();
            int newId = 0;
            if (lastReceipt != null) {
                string id = lastReceipt.ReceiptId;
                newId = Convert.ToInt32(id.Substring(id.Length - 5));
                if (newId < 99999)
                    newId += 1;
                else
                    newId = 1;
            } else {
                newId = 1;
            }
            return $"RC{DateTime.Now.Year.ToString().Substring(2)}" +
                $"{DateTime.Now.Month:D2}" +
                $"{newId:D5}";
        }
    }
}
