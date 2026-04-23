using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public RachaController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // ── GET ALL ──────────────────────────────────────────────────────
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _context.Locations.Include(l => l.User).ToListAsync();
            var result = locations.Select(l => new
            {
                l.Id, l.NameGeo, l.NameEng, l.TypeGeo, l.TypeEng,
                l.Category, l.InfoGeo, l.InfoEng, l.Latitude, l.Longitude,
                l.ImageUrl, l.Description,
                AddedBy = l.User?.Username ?? "System"
            });
            return Ok(result);
        }

        // ── GET SINGLE ───────────────────────────────────────────────────
        [HttpGet("locations/{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            var location = await _context.Locations.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            return Ok(new
            {
                location.Id, location.NameGeo, location.NameEng, location.TypeGeo, location.TypeEng,
                location.Category, location.InfoGeo, location.InfoEng,
                location.Latitude, location.Longitude, location.ImageUrl, location.Description,
                AddedBy = location.User?.Username ?? "System"
            });
        }

        // ── CREATE ───────────────────────────────────────────────────────
        [Authorize]
        [HttpPost("locations")]
        public async Task<IActionResult> AddLocation([FromForm] LocationDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            string? mediaUrl = await UploadToCloudinary(request.Image);

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
                ImageUrl = mediaUrl,
                UserId = userId
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return Ok(location);
        }

        // ── UPDATE ───────────────────────────────────────────────────────
        [Authorize]
        [HttpPut("locations/{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromForm] LocationDto request)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();
            if (location.UserId != userId && role != "Admin" && role != "SuperAdmin")
                return Forbid();

            location.NameGeo = request.NameGeo;
            location.NameEng = request.NameEng ?? request.NameGeo;
            location.TypeGeo = request.TypeGeo ?? location.TypeGeo;
            location.TypeEng = request.TypeEng ?? location.TypeEng;
            location.Category = request.Category;
            location.InfoGeo = request.InfoGeo ?? "";
            location.InfoEng = request.InfoEng ?? "";
            location.Description = request.Description;
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;

            if (request.Image != null)
                location.ImageUrl = await UploadToCloudinary(request.Image);

            await _context.SaveChangesAsync();
            return Ok(location);
        }

        // ── DELETE ───────────────────────────────────────────────────────
        [Authorize]
        [HttpDelete("locations/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();
            if (location.UserId != userId && role != "Admin" && role != "SuperAdmin")
                return Forbid();

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── Cloudinary helper (image + video) ────────────────────────────
        private async Task<string?> UploadToCloudinary(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            await using var stream = file.OpenReadStream();
            var isVideo = file.ContentType.StartsWith("video/");

            if (isVideo)
            {
                var vp = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "racha629",
                    Overwrite = false
                };
                var result = await _cloudinary.UploadAsync(vp);
                return result?.SecureUrl?.ToString();
            }
            else
            {
                var ip = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "racha629",
                    Transformation = new Transformation().Width(1400).Height(900).Crop("limit").Quality("auto:best"),
                    Overwrite = false
                };
                var result = await _cloudinary.UploadAsync(ip);
                return result?.SecureUrl?.ToString();
            }
        }
    }

    public class LocationDto
    {
        public string NameGeo { get; set; } = "";
        public string? NameEng { get; set; }
        public string? TypeGeo { get; set; }
        public string? TypeEng { get; set; }
        public string Category { get; set; } = "";
        public string? InfoGeo { get; set; }
        public string? InfoEng { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IFormFile? Image { get; set; }
    }
}
