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
using PBL3.Service;

namespace PBL3.Controllers {
    [ApiController]
    [Route("[Controller]")]
    public class AuthController : ControllerBase {
        private readonly ShopGuitarContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBlobService _blobService;
        private readonly IEmailService _emailService;
        private const string _containerName = "images";

        //public ActionResult UserTokenProvider { get; private set; }

        public AuthController(ShopGuitarContext context
            , IConfiguration configuration
            , IBlobService blobService
            , IEmailService emailService) {
            _context = context;
            _configuration = configuration;
            _blobService = blobService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto request) {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            string verificationToken = CreateRandomToken();

            Account account = new Account {
                UserName = request.UserName,
                UserId = request.UserId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = verificationToken
            };

            if (!_context.Accounts.Any(a => a.UserName == account.UserName)
                && _context.Users.Any(u => u.Id == account.UserId)) {
                if (request.ImageFile != null) {
                    var res = await SaveImage(request.UserId, request.ImageFile);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);
                if (user == null) {
                    return BadRequest("User does not exist.");
                }

                string email = user.Email;

                await _emailService.SendEmail(new EmailDto {
                    To = email,
                    Subject = "Comfirm your password!",
                    Key = verificationToken
                });
                account.VerifyTokenExpires = DateTime.UtcNow.AddHours(7).AddMinutes(2);

                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return Ok("Your account has been created successfully");
            }

            return BadRequest("Account already exists or UserId not exists");
        }

        [HttpGet("RefreshVerifyToken")]
        public async Task<IActionResult> RefreshVerifyToken(string userId) {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return BadRequest("Account does not exist!");

            account.VerificationToken = CreateRandomToken();
            account.VerifyTokenExpires = DateTime.UtcNow.AddHours(7).AddMinutes(2);

            return Ok("Please check your email");
        }

        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount(string token) {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.VerificationToken == token);
            if (account == null || account.VerifyTokenExpires < DateTime.UtcNow.AddHours(7))
                return BadRequest("Invalid token.");

            account.VerifiedAccountAt = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();

            return Ok("Account verified!");
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

            if (account.VerifiedAccountAt == null)
                return BadRequest("Not verified!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == account.UserId);

            if (user != null) {
                var token = createToken(user);
                return Ok(token);
            }

            return BadRequest("User Not Found!");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) {
                return BadRequest("User not found.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (account == null) {
                return BadRequest("Account not found.");
            }

            account.PasswordResetToken = CreateRandomToken();
            await _emailService.SendEmail(new EmailDto {
                To = user.Email,
                Subject = "Reset Password",
                Key = account.PasswordResetToken
            });
            account.ResetTokenExpires = DateTime.UtcNow.AddHours(7).AddMinutes(2);
            await _context.SaveChangesAsync();

            return Ok("Please check your email");
        }

        [HttpPost("verify-reset-password")]
        public async Task<IActionResult> VerifyResetPassword(string token) {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.PasswordResetToken == token);

            if (account == null || account.ResetTokenExpires < DateTime.UtcNow.AddHours(7)) {
                return BadRequest("Invalid token.");
            }

            account.VerifiedResetPasswordAt = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();

            return Ok("You may now reset your password.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResettPassword(ResetPasswordDto request) {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId);
            if (account == null) {
                return BadRequest("Account does not exist.");
            }

            if (account.VerifiedResetPasswordAt == null) {
                return BadRequest("Invalid Token.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.PasswordResetToken = null;
            account.ResetTokenExpires = null;
            account.VerifiedResetPasswordAt = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request) {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == getCurrentUserId());

            if (account == null) {
                return BadRequest("Account does not Exist.");
            }

            if (!verifyPassWordHash(request.CurrentPassword, account.PasswordHash, account.PasswordSalt))
                return BadRequest("Wrong Password!");

            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return Ok("Password successfully change.");
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
                expires: DateTime.UtcNow.AddHours(7).AddDays(7),
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

        private string CreateRandomToken() {
            return $"{RandomNumberGenerator.GetInt32(0, 1000000):D6}";
        }

        private string getCurrentUserId() {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userClaims = identity.Claims;

            return userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
