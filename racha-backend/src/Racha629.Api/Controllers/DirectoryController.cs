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
    [Route("api/directory")]
    public class DirectoryController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly Cloudinary _cloudinary;

        public DirectoryController(AppDbContext ctx, Cloudinary cloudinary)
        {
            _ctx = ctx;
            _cloudinary = cloudinary;
        }

        // POST submit (public) — multipart/form-data
        [HttpPost]
        public async Task<IActionResult> Submit([FromForm] DirectorySubmissionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest("სახელი სავალდებულოა");

            string? photoUrl = null;

            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                using var stream = dto.Photo.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(dto.Photo.FileName, stream),
                    Folder = "racha629/directory",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                    return BadRequest($"ფოტოს ატვირთვა ვერ მოხერხდა: {uploadResult.Error.Message}");
                photoUrl = uploadResult.SecureUrl?.ToString();
            }

            var sub = new DirectorySubmission
            {
                FullName     = dto.FullName,
                Phone        = dto.Phone ?? "",
                District     = dto.District ?? "",
                Village      = dto.Village ?? "",
                LocationType = dto.LocationType ?? "",
                Latitude     = dto.GetLatitude(),
                Longitude    = dto.GetLongitude(),
                Notes        = dto.Notes,
                Description  = dto.Description,
                PhotoUrl     = photoUrl
            };

            _ctx.DirectorySubmissions.Add(sub);
            await _ctx.SaveChangesAsync();
            return Ok(new { id = sub.Id, status = sub.Status });
        }

        // GET all (Admin)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var list = await _ctx.DirectorySubmissions
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            return Ok(list);
        }

        // PUT update status (Admin)
        [Authorize]
        [HttpPut("{id}/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var allowed = new[] { "Pending", "Approved", "Rejected" };
            if (!allowed.Contains(status)) return BadRequest("Invalid status");

            var sub = await _ctx.DirectorySubmissions.FindAsync(id);
            if (sub == null) return NotFound();
            sub.Status = status;
            await _ctx.SaveChangesAsync();
            return Ok();
        }

        // DELETE (Admin)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var sub = await _ctx.DirectorySubmissions.FindAsync(id);
            if (sub == null) return NotFound();
            _ctx.DirectorySubmissions.Remove(sub);
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }

    public class DirectorySubmissionDto
    {
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? District { get; set; }
        public string? Village { get; set; }
        public string? LocationType { get; set; }
        // Stored as string so FormData decimal dots always parse correctly
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Notes { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }

        public double GetLatitude() => double.TryParse(Latitude,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

        public double GetLongitude() => double.TryParse(Longitude,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }
}
