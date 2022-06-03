using Microsoft.AspNetCore.Mvc;
using PBL3.Models;
using PBL3.DTO;
using System.Security.Cryptography;
using System.Text;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Web.Http.Cors;

namespace PBL3.Controllers {
    [ApiController]
    [Route("[Controller]")]
    public class AuthController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IConfiguration _configuration;

        public ActionResult UserTokenProvider { get; private set; }

        public AuthController(ShopGuitarContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto request) {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            Account account = new Account();
            account.UserName = request.UserName;
            account.UserId = request.UserId;
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;

            if (!_context.Accounts.Any(a => a.UserName == account.UserName) && _context.Users.Any(u => u.Id == account.UserId)) {
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return Ok("Your account has been created successfully");
            }

            return BadRequest("Account already exists or UserId not exists");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(UserLoginDto request) {
            if (!_context.Accounts.Any(a => a.UserName == request.UserName)) {
                return BadRequest("User Name not Found");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserName == request.UserName);

            if (account != null) {
                if (!verifyPassWordHash(request.Password, account.PasswordHash, account.PasswordSalt)) {
                    return BadRequest("Wrong Password");
                }
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);

            if (user != null) { 
                var token = createToken(user);
                return Ok(token);
            }

            return BadRequest("User Not Found!");
        }

        private string createToken(User user) {
            string gender = user.Gender ? "Male" : "Female";

            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Gender, gender),
                new Claim(ClaimTypes.DateOfBirth, Convert.ToString(user.DateOfBirth)),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.StreetAddress, user.Address),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using (var hmac = new HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool verifyPassWordHash(string password, byte[] passwordHash, byte[] passwordSalt) { 
            using (var hmac = new HMACSHA512(passwordSalt)) {
                var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
