using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.DTO;
using System.Security.Claims;

namespace PBL3.Controllers {
    [Route("api/[Controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase {
        private readonly ShopGuitarContext _context;

        public EmployeeController(ShopGuitarContext context) {
            _context = context;
        }

        [HttpGet("employee")]
        [Authorize]
        public async Task<ActionResult<List<Employee>>> GetAllEmployee() {
            var result = await _context
                .Employees
                .Include(e => e.User)
                .Include(e => e.Titles)
                .Include(e => e.Salaries)
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("employee/{id}")]
        [Authorize]
        public async Task<ActionResult<Employee>> GetEmployee(string id) {
            var emp = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Titles)
                .Include(e => e.Salaries)
                .FirstOrDefaultAsync(e => e.Id == id);
            return Ok(emp);
        }

        [HttpGet("current-employee")]
        [Authorize]
        public async Task<ActionResult> GetCurrentEmployee() {
            string id = GetCurrentUser().Id;
            var emp = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Titles)
                .Include(e => e.Salaries)
                .FirstOrDefaultAsync();
            return Ok(emp);
        }

        [HttpPut("update-employee")]
        [Authorize]
        public async Task<ActionResult> PutEmployee(EmployeeDto emp) {
            var employee = _context.Employees.Find(emp.EmployeeId);
            var user = _context.Users.Find(emp.EmployeeId);
            if (employee == null || user == null) {
                return BadRequest("Employee not found!");
            }

            user.FirstName = emp.FirstName;
            user.LastName = emp.LastName;
            user.Gender = emp.Gender;
            user.DateOfBirth = emp.DateOfBirth;
            user.PhoneNumber = emp.PhoneNumber;
            user.Email = emp.Email;
            user.Address = emp.Address;

            await _context.SaveChangesAsync();

            return Ok("Updated!");
        }

        [HttpPut("update-employee-admin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> updateEmployee(EmployeeDto emp) {
            var employee = _context.Employees.Find(emp.EmployeeId);
            var user = _context.Users.Find(emp.EmployeeId);
            var title = _context.Titles.Find(emp.EmployeeId);
            var salary = _context.Salaries.Find(emp.EmployeeId);
            if (employee == null || user == null || title == null) {
                return BadRequest("Employee not found!");
            }

            employee.ManagerId = emp.ManagerId;
            user.FirstName = emp.FirstName;
            user.LastName = emp.LastName;
            user.Gender = emp.Gender;
            user.DateOfBirth = emp.DateOfBirth;
            user.PhoneNumber = emp.PhoneNumber;
            user.Email = emp.Email;
            user.Address = emp.Address;
            title.Name = emp.TitleName;
            title.DateIn = emp.DateIn;
            title.DateOut = emp.DateOut;
            salary.Salary = emp.slary;

            await _context.SaveChangesAsync();

            return Ok("Updated!");
        }

        [HttpPost("add-employee")]
        [Authorize("admin")]
        public async Task<ActionResult> AddEmployee(EmployeeDto emp) {
            string employeeId = await autoGenerationEmployeeId(emp);
            string managerId = emp.ManagerId;
            if (emp.Role == "admin")
                managerId = employeeId;

            Employee employee = new Employee {
                Id = employeeId,
                ManagerId = managerId
            };
            User user = new User {
                Id = employeeId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,
                PhoneNumber = emp.PhoneNumber,
                Email = emp.Email,
                Address = emp.Address,
                Role = emp.Role
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
                Salary = emp.slary,
                FromDate = fromDate,
                ToDate = fromDate.AddMonths(1)
            };

            await _context.Employees.AddAsync(employee);
            await _context.Users.AddAsync(user);
            await _context.Titles.AddAsync(title);
            await _context.Salaries.AddAsync(salarie);

            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpDelete("delete-employee/{id}")]
        [Authorize("admin")]
        public async Task<ActionResult> DeleteEmployee(string id) { 
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
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
    }
}
