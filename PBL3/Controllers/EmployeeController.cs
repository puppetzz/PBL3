using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.DTO;
using System.Security.Claims;
using PBL3.Service;
using OfficeOpenXml;

namespace PBL3.Controllers {
    [Route("api/[Controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IBlobService _blobService;
        private readonly IExcelService _excelService;
        private const string _containerName = "images";

        public EmployeeController(ShopGuitarContext context, IBlobService blobService, IExcelService excelService) {
            _context = context;
            _blobService = blobService;
            _excelService = excelService;
        }

        [HttpGet("employee")]
        [Authorize]
        public async Task<ActionResult> GetAllEmployee() {
            var result = await _context
                .Employees
                .Include(e => e.User)
                .Select(e => new {
                    e.Id,
                    e.User.FirstName,
                    e.User.LastName,
                    e.User.Gender,
                    e.User.DateOfBirth,
                    e.User.PhoneNumber,
                    e.User.Email,
                    e.User.Address,
                    e.User.Role,
                    e.ManagerId,
                    ImageUri = e.User.ImageName == null ? null : _blobService.GetBlob(e.User.ImageName, _containerName),
                    e.TitleName,
                    e.Salary,
                    e.DateIn,
                    e.DateOut
                })
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("employee/{id}")]
        [Authorize]
        public async Task<ActionResult> GetEmployee(string id) {
            var emp = await _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    e.Id,
                    e.User.FirstName,
                    e.User.LastName,
                    e.User.Gender,
                    e.User.DateOfBirth,
                    e.User.PhoneNumber,
                    e.User.Email,
                    e.User.Address,
                    e.User.Role,
                    e.ManagerId,
                    ImageUri = e.User.ImageName == null ? null : _blobService.GetBlob(e.User.ImageName, _containerName),
                    e.TitleName,
                    e.Salary,
                    e.DateIn,
                    e.DateOut
                })
                .FirstOrDefaultAsync(e => e.Id == id);
            return Ok(emp);
        }

        [HttpGet("current-employee")]
        [Authorize]
        public async Task<ActionResult> GetCurrentEmployee() {
            string id = GetCurrentUser().Id;
            var emp = await _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    e.Id,
                    e.User.FirstName,
                    e.User.LastName,
                    e.User.Gender,
                    e.User.DateOfBirth,
                    e.User.PhoneNumber,
                    e.User.Email,
                    e.User.Address,
                    e.User.Role,
                    e.ManagerId,
                    ImageUri = e.User.ImageName == null ? null : _blobService.GetBlob(e.User.ImageName, _containerName),
                    e.TitleName,
                    e.Salary,
                    e.DateIn,
                    e.DateOut
                })
                .FirstOrDefaultAsync(e => e.Id == id);
            return Ok(emp);
        }

        [HttpPut("update-employee")]
        [Authorize]
        public async Task<ActionResult> PutEmployee([FromForm] EmployeeUpdateDto emp) {
            var employee = _context.Employees.Find(emp.EmployeeId);

            var user = _context.Users.Find(emp.EmployeeId);

            if (employee == null || user == null)
                return BadRequest("Employee not found!");

            bool res = false;

            if (emp.ImageFile != null) {
                _ = await _blobService.DeleteBlobAsync(user.ImageName, _containerName);

                res = await SaveImage(emp.EmployeeId, emp.ImageFile);

                if (!res) {
                    return BadRequest("Image upload failed");
                }
            }

            user.FirstName = emp.FirstName;
            user.LastName = emp.LastName;
            user.Gender = emp.Gender;
            user.DateOfBirth = emp.DateOfBirth;
            user.PhoneNumber = emp.PhoneNumber;
            user.Email = emp.Email;
            user.Address = emp.Address;

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Update successful",
                ImageAdded = res
            });
        }

        [HttpPut("update-employee-admin")]
        [Authorize(Roles = "admin, 0")]
        public async Task<ActionResult> updateEmployee([FromForm] EmployeeDto emp) {
            var employee = _context.Employees.Find(emp.EmployeeId);

            var user = _context.Users.Find(emp.EmployeeId);

            if (employee == null || user == null) {
                return BadRequest("Employee not found!");
            }

            bool res = false;

            if (emp.ImageFile != null) {
                _ = await _blobService.DeleteBlobAsync(user.ImageName, _containerName);

                res = await SaveImage(emp.EmployeeId, emp.ImageFile);

                if (!res) {
                    return BadRequest("Image upload failed");
                }
            }

            employee.ManagerId = emp.ManagerId;
            user.FirstName = emp.FirstName;
            user.LastName = emp.LastName;
            user.Gender = emp.Gender;
            user.DateOfBirth = emp.DateOfBirth;
            user.PhoneNumber = emp.PhoneNumber;
            user.Email = emp.Email;
            user.Address = emp.Address;
            employee.TitleName = emp.TitleName;
            employee.Salary = emp.salary;
            employee.DateIn = emp.DateIn;
            employee.DateOut = emp.DateOut;
            if (employee.User.Role != "0" && GetCurrentUser().Role == "0")
                user.Role = emp.Role.ToLower();

            if (GetCurrentUser().Role == "admin" && employee.User.Role == "employee") {
                user.Role = emp.Role.ToLower();
            }

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Update successful",
                ImageAdded = res
            });
        }

        [HttpPost("add-employee")]
        [Authorize(Roles = "admin, 0")]
        public async Task<ActionResult> AddEmployee([FromForm] AddEmployeeDto emp) {
            string employeeId = await autoGenerationEmployeeId(emp);

            string managerId = emp.ManagerId;

            if (emp.Role == "admin")
                managerId = employeeId;

            Employee employee = new Employee {
                Id = employeeId,
                ManagerId = managerId,
                TitleName = emp.TitleName,
                Salary = emp.salary,
                DateIn = emp.DateIn,
                DateOut = emp.DateOut,
            };

            string? fileName = null;

            bool res = false;

            if (emp.ImageFile != null && emp.ImageFile.Length > 1) {
                fileName = Guid.NewGuid() + Path.GetExtension(emp.ImageFile.FileName);

                res = await _blobService.UploadBlobAsync(fileName, emp.ImageFile, _containerName);
            }

            User user = new User {
                Id = employeeId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,
                PhoneNumber = emp.PhoneNumber,
                Email = emp.Email,
                Address = emp.Address,
                Role = emp.Role.ToLower(),
                ImageName = fileName
            };

            await _context.Users.AddAsync(user);
            await _context.Employees.AddAsync(employee);

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Add successfull",
                ImageAdded = res
            });
        }

        [HttpPost("add-employee-from-excel-file")]
        [Authorize(Roles = "admin, 0")]
        public async Task<ActionResult> ImportEmployeeFromExcel(IFormFile file) {
            List<AddEmployeeDto> list = new List<AddEmployeeDto>();

            try {
                list = await _excelService.GetEmployeeFromExcelAsync(file);
            } catch (Exception e) {
                return BadRequest(e.Message);
            }

            foreach (AddEmployeeDto emp in list) {
                if (_context.Users.Any(u => u.Email == emp.Email || u.PhoneNumber == emp.PhoneNumber))
                    return BadRequest("The Excel file exists for the previously added employee");
            }

            foreach (AddEmployeeDto emp in list) {

                string employeeId = await autoGenerationEmployeeId(emp);

                string managerId = emp.ManagerId;

                if (emp.Role == "admin")
                    managerId = employeeId;

                Employee employee = new Employee {
                    Id = employeeId,
                    ManagerId = managerId,
                    TitleName = emp.TitleName,
                    Salary = emp.salary,
                    DateIn = emp.DateIn,
                    DateOut = emp.DateOut,
                };

                string? fileName = null;

                bool res = false;

                if (emp.ImageFile != null && emp.ImageFile.Length > 1) {
                    fileName = Guid.NewGuid() + Path.GetExtension(emp.ImageFile.FileName);

                    res = await _blobService.UploadBlobAsync(fileName, emp.ImageFile, _containerName);
                }

                User user = new User {
                    Id = employeeId,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    Gender = emp.Gender,
                    DateOfBirth = emp.DateOfBirth,
                    PhoneNumber = emp.PhoneNumber[0] == '0' ? emp.PhoneNumber : $"0{emp.PhoneNumber}",
                    Email = emp.Email,
                    Address = emp.Address,
                    Role = emp.Role.ToLower(),
                    ImageName = fileName
                };

                await _context.Employees.AddAsync(employee);
                await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();

            }
            return Ok(new {
                status = "Add successfull",
            });
        }

        [HttpDelete("delete-employee/{id}")]
        [Authorize(Roles = "admin, 0")]
        public async Task<ActionResult> DeleteEmployee(string id) {
            var user = await _context.Users.FindAsync(id);

            if (user.Role == "0")
                return BadRequest("This account cannot be deleted!");

            if (user.Role == "admin" && GetCurrentUser().Role == "admin")
                return BadRequest("Admin cannot be deleted by Admin!");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == id);

            if (user == null)
                return BadRequest("Employee doesn't exist!");

            bool? res = null;

            if (user.ImageName != null)
                res = await _blobService.DeleteBlobAsync(user.ImageName, _containerName);

            _context.Users.Remove(user);

            if (account != null)
                _context.Accounts.Remove(account);

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Delete successful",
                ImageDeleted = res
            });
        }

        [HttpGet("export-employee-to-excel")]
        [Authorize]
        public async Task<ActionResult> ExportEmployee() {
            var employee = await _context.Users
                .ToListAsync();

            byte[] fileContents;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Employee");
            Sheet.Cells["A1"].Value = "Employee ID";
            Sheet.Cells["B1"].Value = "Manager ID";
            Sheet.Cells["C1"].Value = "First Name";
            Sheet.Cells["D1"].Value = "Last Name";
            Sheet.Cells["E1"].Value = "Gender";
            Sheet.Cells["F1"].Value = "Date Of Birth";
            Sheet.Cells["G1"].Value = "Phone Number";
            Sheet.Cells["H1"].Value = "Email";
            Sheet.Cells["K1"].Value = "Address";
            Sheet.Cells["L1"].Value = "Role";

            int row = 2;
            foreach (var item in employee) {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.Id;
                Sheet.Cells[string.Format("B{0}", row)].Value = _context.Employees
                    .Where(e => e.Id == item.Id)
                    .Select(e => e.ManagerId)
                    .FirstOrDefault().ToString();
                Sheet.Cells[string.Format("C{0}", row)].Value = item.FirstName;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.LastName;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Gender ? "Male" : "Female";
                Sheet.Cells[string.Format("F{0}", row)].Value = item.DateOfBirth;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.PhoneNumber;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.Email;
                Sheet.Cells[string.Format("K{0}", row)].Value = item.Address;
                Sheet.Cells[string.Format("L{0}", row)].Value = item.Role;
                row++;
            }

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileContents = Ep.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0) {
                return NotFound();
            }

            string excelName = $"Employee-List-{DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff")}.xlsx";

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: excelName
            );
        }

        private async Task<string> autoGenerationEmployeeId(AddEmployeeDto employeeDto) {
            var lastEmployee = await _context.Employees.OrderByDescending(e => e.Id).FirstOrDefaultAsync();
            int newId = 0;
            if (lastEmployee != null) {
                string id = lastEmployee.Id;
                newId = Convert.ToInt32(id.Substring(id.Length - 3));
                if (newId < 999)
                    newId += 1;
                else
                    newId = 1;
            } else {
                newId = 1;
            }
            string roleCode = employeeDto.Role == "admin" ? "001" : "010";
            return $"{DateTime.Now.Year.ToString().Substring(2)}0{roleCode}{newId:D3}";
        }

        private User GetCurrentUser() {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null) {
                var userClaims = identity.Claims;

                return new User {
                    Id = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    FirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    LastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Gender = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Gender)?.Value == "Male",
                    DateOfBirth = Convert.ToDateTime(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.DateOfBirth)?.Value),
                    Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    PhoneNumber = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.MobilePhone)?.Value,
                    Address = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.StreetAddress)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }

        private async Task<bool> SaveImage(string userId, IFormFile file) {
            var user = _context.Users.Find(userId);

            if (file == null || file.Length < 1)
                return false;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var res = await _blobService.UploadBlobAsync(fileName, file, _containerName);

            if (res) {
                user.ImageName = fileName;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
