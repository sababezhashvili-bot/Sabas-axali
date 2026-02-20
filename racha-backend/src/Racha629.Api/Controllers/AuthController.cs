using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Racha629.Api.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Racha629.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
            {
                return BadRequest("User or Email already exists.");
            }

            // Create User
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Username.ToLower() == "admin" ? "SuperAdmin" : "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Initialize Permissions
            var perms = new Permission
            {
                User = user,
                CanViewUsers = user.Role == "Admin" || user.Role == "SuperAdmin",
                CanEditServices = user.Role == "Admin" || user.Role == "SuperAdmin",
                CanDeleteData = user.Role == "SuperAdmin"
            };

            // Activity Log
            var log = new ActivityLog
            {
                User = user,
                Action = "Register",
                ServiceType = "Auth",
                Description = $"User {user.Username} registered.",
                Timestamp = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Permissions.Add(perms);
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Username, user.Role });
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(UserDto request)
        {
            // Hardcoded Admin Logic
            if (request.Username == "Admin" && request.Password == "admin123")
            {
                var adminUser = await _context.Users.Include(u => u.Permission).FirstOrDefaultAsync(u => u.Username == "Admin");
                if (adminUser == null)
                {
                    // Create Admin if not exists
                    adminUser = new User
                    {
                        Username = "Admin",
                        Email = "admin@racha.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "SuperAdmin",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    var perms = new Permission
                    {
                        User = adminUser,
                        CanViewUsers = true,
                        CanEditServices = true,
                        CanDeleteData = true
                    };
                    _context.Users.Add(adminUser);
                    _context.Permissions.Add(perms);
                    await _context.SaveChangesAsync();
                }
                else if (adminUser.Role != "SuperAdmin")
                {
                    adminUser.Role = "SuperAdmin"; // Enforce Role
                    await _context.SaveChangesAsync();
                }

                // Proceed to login
                return await GenerateLoginResponse(adminUser);
            }

            var user = await _context.Users.Include(u => u.Permission).FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !user.IsActive)
            {
                return BadRequest("User not found or inactive.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            return await GenerateLoginResponse(user);
        }

        private async Task<ActionResult<object>> GenerateLoginResponse(User user)
        {
             // Log Login
            var log = new ActivityLog
            {
                UserId = user.Id,
                Action = "Login",
                ServiceType = "Auth",
                Description = "User logged in.",
                Timestamp = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            string token = CreateToken(user);
            
            return Ok(new 
            { 
                token, 
                user.Id, 
                user.Username, 
                user.Role,
                permissions = user.Permission 
            });
        }
        
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
             var identity = HttpContext.User.Identity as ClaimsIdentity;
             if (identity != null)
             {
                 var usernameClaim = identity.FindFirst(ClaimTypes.Name);
                 var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                 
                 if (usernameClaim != null && idClaim != null)
                 {
                      int userId = int.Parse(idClaim.Value);
                      var user = await _context.Users.Include(u => u.Permission).FirstOrDefaultAsync(u => u.Id == userId);
                      if(user == null) return Unauthorized();

                      return Ok(new { 
                          id = user.Id,
                          username = user.Username, 
                          role = user.Role,
                          permissions = user.Permission
                      });
                 }
             }
             return Unauthorized();
        }


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }

    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
