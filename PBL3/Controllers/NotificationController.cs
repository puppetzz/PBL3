using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.DTO;
using PBL3.Models;
using System.Security.Claims;

namespace PBL3.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase {
        private readonly ShopGuitarContext _context;

        public NotificationController(ShopGuitarContext context) {
            _context = context;
        }

        [HttpGet("notification")]
        [Authorize]
        public async Task<IActionResult> GetAllNotification() {
            var notification = await _context.Notifications
                .Include(n => n.Manager.User)
                .Select(n => new {
                    n.NotificationId,
                    n.TitleName,
                    n.Content,
                    n.DatePost,
                    ManagerPost = n.Manager.User,
                    n.DateUpdate,
                    ManagerUpdate = n.ManagerIdUpdated == null 
                        ? null : _context.Users.FirstOrDefault(u => u.Id == n.ManagerIdUpdated)
                }).OrderByDescending(n => n.DatePost)
                .ThenByDescending(n => n.NotificationId)
                .ToListAsync();

            if (notification == null)
                return BadRequest("Notifications are't exists!");

            return Ok(notification);
        }

        [HttpGet("notification/{id}")]
        [Authorize]
        public async Task<IActionResult> getNotification(string id) {
            var notification = await _context.Notifications
                .Include(n => n.Manager.User)
                .Select(n => new {
                    n.NotificationId,
                    n.TitleName,
                    n.Content,
                    n.DatePost,
                    ManagerPost = n.Manager.User,
                    n.DateUpdate,
                    ManagerUpdate = n.ManagerIdUpdated == null 
                        ? null : _context.Users.FirstOrDefault(u => u.Id == n.ManagerIdUpdated)
                }).FirstOrDefaultAsync(n => n.NotificationId == id);
            if (notification == null)
                return BadRequest("Notification are't exist!");

            return Ok(notification);
        }

        [HttpPost("add-notification")]
        [Authorize(Roles ="admin, 0")]
        public async Task<ActionResult> AddNotification(NotificationAddDto notificationDto) {
            Notification notification = new Notification {
                NotificationId = await generationNewNotificationId(),
                ManagerIdPost = getCurrentEmployeeId(),
                TitleName = notificationDto.TitleName,
                Content = notificationDto.Content,
                DatePost = DateTime.UtcNow.AddHours(7)
            };
            
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpPut("update-notification")]
        [Authorize(Roles ="admin, 0")]
        public async Task<ActionResult> UpdateNotification(NotificationUpdateDto notificationDto) {
            if (notificationDto.NotificationId == null)
                return BadRequest("NotificationId Can't be Null!");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationDto.NotificationId);

            if (notification == null)
                return BadRequest("Notification doesn't exist!");
            
            notification.TitleName = notificationDto.TitleName;
            notification.Content = notificationDto.Content;
            notification.DateUpdate = DateTime.UtcNow.AddHours(7);
            notification.ManagerIdUpdated = getCurrentEmployeeId();

            await _context.SaveChangesAsync();

            return Ok("Updated!");
        }

        [HttpDelete("delete-notification/{id}")]
        [Authorize(Roles ="admin, 0")]
        public async Task<ActionResult> DeleteNotificaton(string id) {
            var notificaton = await _context.Notifications.FindAsync(id);

            if (notificaton == null)
                return BadRequest("Notification doesn't exist!");

            _context.Notifications.Remove(notificaton);
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }

        private async Task<string> generationNewNotificationId() {
            var lastNotification = await _context.Notifications.OrderByDescending(r => r.NotificationId).FirstOrDefaultAsync();
            int newId = 0;
            if (lastNotification != null) {
                string id = lastNotification.NotificationId;
                newId = Convert.ToInt32(id.Substring(id.Length - 5));
                if (newId < 99999)
                    newId += 1;
                else
                    newId = 1;
            } else {
                newId = 1;
            }
            return $"NO{DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2)}0{newId:D5}";
        }

        private string getCurrentEmployeeId() {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userClaim = identity.Claims;

            return userClaim.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
