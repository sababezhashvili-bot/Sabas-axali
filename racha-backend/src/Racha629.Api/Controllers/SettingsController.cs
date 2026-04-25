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
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;
        public SettingsController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // ── GET ALL (public) ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var all = await _context.SiteSettings.ToListAsync();
            var dict = all.ToDictionary(s => s.Key, s => s.Value);
            return Ok(dict);
        }

        // ── UPDATE (Admin only) ──────────────────────────────────
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> updates)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            foreach (var kv in updates)
            {
                var existing = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == kv.Key);
                if (existing != null)
                    existing.Value = kv.Value;
                else
                    _context.SiteSettings.Add(new SiteSetting { Key = kv.Key, Value = kv.Value });
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── UPLOAD IMAGE (Admin only) ────────────────────────────
        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image, [FromForm] string key)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();
            if (image == null || image.Length == 0) return BadRequest("No image");

            var uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                Folder = "racha629/settings"
            });
            var url = uploadResult.SecureUrl.ToString();

            var existing = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (existing != null)
                existing.Value = url;
            else
                _context.SiteSettings.Add(new SiteSetting { Key = key, Value = url });

            await _context.SaveChangesAsync();
            return Ok(new { url });
        }
    }
}
