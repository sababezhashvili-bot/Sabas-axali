using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;
using System.Security.Claims;

namespace Racha629.Api.Controllers
{
    [ApiController]
    [Route("api/racha")]
    public class RachaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RachaController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            try 
            {
                var locations = await _context.Locations.Include(l => l.User).ToListAsync();
                var result = locations.Select(l => new
                {
                    l.Id,
                    l.NameGeo,
                    l.NameEng,
                    l.TypeGeo,
                    l.TypeEng,
                    l.Category,
                    l.InfoGeo,
                    l.InfoEng,
                    l.Latitude,
                    l.Longitude,
                    l.ImageUrl,
                    l.Description,
                    AddedBy = l.User?.Username ?? "System"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("locations")]
        public async Task<IActionResult> AddLocation([FromForm] LocationDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            
            int userId = int.Parse(userIdClaim);

            string? imageUrl = null;
            if (request.Image != null)
            {
                // სწორი გზა wwwroot/images-მდე
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploads = Path.Combine(webRoot, "images");
                
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
                var filePath = Path.Combine(uploads, fileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(fileStream);
                }
                imageUrl = "/images/" + fileName;
            }

            var location = new Location
            {
                NameGeo = request.NameGeo,
                NameEng = request.NameEng ?? request.NameGeo,
                TypeGeo = request.TypeGeo ?? "ადგილი",
                TypeEng = request.TypeEng ?? "Place",
                Category = request.Category,
                InfoGeo = request.InfoGeo ?? "",
                InfoEng = request.InfoEng ?? "",
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ImageUrl = imageUrl,
                UserId = userId
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return Ok(location);
        }

        [Authorize]
        [HttpDelete("locations/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
             var role = User.FindFirst(ClaimTypes.Role)?.Value;

             var location = await _context.Locations.FindAsync(id);
             if (location == null) return NotFound();

             if (location.UserId != userId && role != "Admin")
             {
                 return Forbid();
             }

             _context.Locations.Remove(location);
             await _context.SaveChangesAsync();
             return Ok();
        }
    }

    public class LocationDto
    {
        public string NameGeo { get; set; }
        public string? NameEng { get; set; }
        public string? TypeGeo { get; set; }
        public string? TypeEng { get; set; }
        public string Category { get; set; }
        public string? InfoGeo { get; set; }
        public string? InfoEng { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IFormFile? Image { get; set; }
    }
}