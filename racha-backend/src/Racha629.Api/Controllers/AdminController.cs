using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Racha629.Api.Data;
using System.Security.Claims;

namespace Racha629.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Only return necessary info
            return await _context.Users
                .Include(u => u.Permission)
                .Select(u => new User 
                { 
                    Id = u.Id, 
                    Username = u.Username, 
                    Email = u.Email, 
                    Role = u.Role, 
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Permission = u.Permission 
                }) // Avoid sending PasswordHash
                .ToListAsync();
        }

        [HttpPost("promote/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> PromoteUser(int id, [FromBody] PermissionDto request)
        {
            var user = await _context.Users.Include(u => u.Permission).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound("User not found");

            user.Role = request.Role; // Admin or User
            
            if (user.Permission == null)
            {
                user.Permission = new Permission { UserId = user.Id };
            }

            user.Permission.CanViewUsers = request.CanViewUsers;
            user.Permission.CanEditServices = request.CanEditServices;
            user.Permission.CanDeleteData = request.CanDeleteData;

            // Log Activity
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = currentUserId,
                Action = "Promote",
                ServiceType = "Admin",
                Description = $"Promoted {user.Username} to {request.Role} with permissions.",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated successfully" });
        }

        [HttpGet("logs")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<IEnumerable<object>>> GetLogs()
        {
            return await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new 
                {
                    l.Id,
                    Username = l.User.Username,
                    l.Action,
                    l.ServiceType,
                    l.Description,
                    l.Timestamp
                })
                .ToListAsync();
        }
    }

    public class PermissionDto
    {
        public string Role { get; set; }
        public bool CanViewUsers { get; set; }
        public bool CanEditServices { get; set; }
        public bool CanDeleteData { get; set; }
    }
}
