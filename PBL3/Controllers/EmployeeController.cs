using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.DTO;
using System.Security.Claims;
using PBL3.Service;

namespace PBL3.Controllers {
    [Route("api/[Controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IBlobService _blobService;
        private const string _containerName = "images";

        public EmployeeController(ShopGuitarContext context, IBlobService blobService) {
            _context = context;
            _blobService = blobService;
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
                    Title = _context.Titles
                    .Where(t => t.EmployeeId == e.Id)
                    .Select(t => new {
                        t.Name,
                        t.DateIn,
                        t.DateOut
                    }).ToList(),
                    Salary = _context.Salaries
                    .Where(s => s.Id == e.Id)
                    .Select(s => new {
                        s.Salary
                    }).ToList()
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
                    Title = _context.Titles
                    .Where(t => t.EmployeeId == e.Id)
                    .Select(t => new {
                        t.Name,
                        t.DateIn,
                        t.DateOut
                    }).ToList(),
                    Salary = _context.Salaries
                    .Where(s => s.Id == e.Id)
                    .Select(s => new {
                        s.Salary
                    }).ToList()
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
                    Title = _context.Titles
                    .Where(t => t.EmployeeId == e.Id)
                    .Select(t => new {
                        t.Name,
                        t.DateIn,
                        t.DateOut
                    }).ToList(),
                    Salary = _context.Salaries
                    .Where(s => s.Id == e.Id)
                    .Select(s => new {
                        s.Salary
                    }).ToList()
                })
                .FirstOrDefaultAsync(e => e.Id == id);
            return Ok(emp);
        }

        [HttpPut("update-employee")]
        [Authorize]
        public async Task<ActionResult> PutEmployee([FromForm]EmployeeUpdateDto emp) {
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
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> updateEmployee([FromForm]EmployeeDto emp) {
            var employee = _context.Employees.Find(emp.EmployeeId);

            var user = _context.Users.Find(emp.EmployeeId);

            var title = _context.Titles.Find(emp.EmployeeId);

            var salary = _context.Salaries.Find(emp.EmployeeId);

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

            if (title == null) {
                await _context.Titles.AddAsync(new Titles {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.TitleName,
                    DateIn = emp.DateIn,
                    DateOut = emp.DateOut
                });
            } else {
                title.Name = emp.TitleName;
                title.DateIn = emp.DateIn;
                title.DateOut = emp.DateOut;
            }

            if (salary == null) {
                await _context.Salaries.AddAsync(new Salaries {
                    Id = emp.EmployeeId,
                    Salary = emp.salary,
                    FromDate = DateTime.UtcNow.AddHours(7),
                    ToDate = DateTime.UtcNow.AddHours(7).AddMonths(1)
                });
            } else {
                salary.Salary = emp.salary;
            }


            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Update successful",
                ImageAdded = res
            });
        }

        [HttpPost("add-employee")]
        [Authorize(Roles="admin")]
        public async Task<ActionResult> AddEmployee([FromForm]EmployeeDto emp) {
            string employeeId = await autoGenerationEmployeeId(emp);

            string managerId = emp.ManagerId;

            if (emp.Role == "admin")
                managerId = employeeId;

            Employee employee = new Employee {
                Id = employeeId,
                ManagerId = managerId
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
            Titles title = new Titles {
                EmployeeId = employeeId,
                Name = emp.TitleName,
                DateIn = emp.DateIn,
                DateOut = emp.DateOut
            };
            DateTime fromDate = new DateTime(DateTime.Now.Year, emp.DateIn.Month, emp.DateIn.Day);
            Salaries salarie = new Salaries {
                Id = employeeId,
                Salary = emp.salary,
                FromDate = fromDate,
                ToDate = fromDate.AddMonths(1)
            };

            await _context.Employees.AddAsync(employee);
            await _context.Users.AddAsync(user);
            await _context.Titles.AddAsync(title);
            await _context.Salaries.AddAsync(salarie);

            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Add successfull",
                ImageAdded = res
            });
        }

        [HttpDelete("delete-employee/{id}")]
        [Authorize(Roles="admin")]
        public async Task<ActionResult> DeleteEmployee(string id) { 
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return BadRequest("Employee doesn't exist!");

            bool? res = null;

            if (user.ImageName != null)
                res = await _blobService.DeleteBlobAsync(user.ImageName, _containerName);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new {
                status = "Delete successful",
                ImageDeleted = res
            });
        }

        private async Task<string> autoGenerationEmployeeId(EmployeeDto employeeDto) {
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
