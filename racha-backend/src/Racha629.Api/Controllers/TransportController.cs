using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;
using System.Security.Claims;

namespace Racha629.Api.Controllers
{
    [ApiController]
    [Route("api/transport")]
    public class TransportController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public TransportController(AppDbContext ctx) { _ctx = ctx; }

        // POST book transport (public)
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] TransportBookingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest("სახელი და ტელეფონი სავალდებულოა");

            var price = dto.VehicleType == "comfort"
                ? 30 + dto.DistanceKm * 2.5
                : 20 + dto.DistanceKm * 2;

            var booking = new TransportBooking
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                FromLocation = dto.FromLocation,
                ToLocation = dto.ToLocation,
                DistanceKm = dto.DistanceKm,
                VehicleType = dto.VehicleType ?? "taxi",
                Price = Math.Round(price, 0),
                Notes = dto.Notes
            };

            _ctx.TransportBookings.Add(booking);
            await _ctx.SaveChangesAsync();
            return Ok(new { id = booking.Id, price = booking.Price, status = booking.Status });
        }

        // GET all bookings (Admin)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var list = await _ctx.TransportBookings
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return Ok(list);
        }

        // PUT update booking status (Admin)
        [Authorize]
        [HttpPut("{id}/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var allowed = new[] { "Pending", "Confirmed", "Cancelled" };
            if (!allowed.Contains(status)) return BadRequest("Invalid status");

            var booking = await _ctx.TransportBookings.FindAsync(id);
            if (booking == null) return NotFound();
            booking.Status = status;
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }

    public class TransportBookingDto
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string FromLocation { get; set; } = "";
        public string ToLocation { get; set; } = "";
        public double DistanceKm { get; set; }
        public string? VehicleType { get; set; } = "taxi";
        public string? Notes { get; set; }
    }
}
