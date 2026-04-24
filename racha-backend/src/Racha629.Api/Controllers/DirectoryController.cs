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
        public DirectoryController(AppDbContext ctx) { _ctx = ctx; }

        // POST submit (public)
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] DirectorySubmissionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest("სახელი სავალდებულოა");

            var sub = new DirectorySubmission
            {
                FullName = dto.FullName,
                District = dto.District,
                Village = dto.Village,
                LocationType = dto.LocationType,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Notes = dto.Notes
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
        public string District { get; set; } = "";
        public string Village { get; set; } = "";
        public string LocationType { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Notes { get; set; }
    }
}
