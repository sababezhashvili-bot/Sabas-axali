using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;
using System.Security.Claims;

namespace Racha629.Api.Controllers
{
    [ApiController]
    [Route("api/tours")]
    public class TourController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public TourController(AppDbContext ctx) { _ctx = ctx; }

        // GET all tours (public)
        [HttpGet]
        public async Task<IActionResult> GetTours()
        {
            var tours = await _ctx.TourPackages
                .Include(t => t.Waypoints.OrderBy(w => w.OrderIndex))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(tours);
        }

        // GET single tour
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTour(int id)
        {
            var t = await _ctx.TourPackages
                .Include(t => t.Waypoints.OrderBy(w => w.OrderIndex))
                .FirstOrDefaultAsync(t => t.Id == id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        // POST create tour (Admin)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTour([FromBody] TourPackageDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var tour = new TourPackage
            {
                NameGeo = dto.NameGeo,
                DescriptionGeo = dto.DescriptionGeo,
                Price = dto.Price,
                DurationHours = dto.DurationHours,
                ImageUrl = dto.ImageUrl,
                Waypoints = (dto.Waypoints ?? new()).Select((w, i) => new TourWaypoint
                {
                    Name = w.Name,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    OrderIndex = i
                }).ToList()
            };

            _ctx.TourPackages.Add(tour);
            await _ctx.SaveChangesAsync();
            return Ok(tour);
        }

        // PUT update tour (Admin)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] TourPackageDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var tour = await _ctx.TourPackages
                .Include(t => t.Waypoints)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            tour.NameGeo = dto.NameGeo;
            tour.DescriptionGeo = dto.DescriptionGeo;
            tour.Price = dto.Price;
            tour.DurationHours = dto.DurationHours;
            tour.ImageUrl = dto.ImageUrl;

            // Replace waypoints
            _ctx.TourWaypoints.RemoveRange(tour.Waypoints);
            tour.Waypoints = (dto.Waypoints ?? new()).Select((w, i) => new TourWaypoint
            {
                TourPackageId = id,
                Name = w.Name,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                OrderIndex = i
            }).ToList();

            await _ctx.SaveChangesAsync();
            return Ok(tour);
        }

        // DELETE tour (Admin)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin" && role != "SuperAdmin") return Forbid();

            var tour = await _ctx.TourPackages.FindAsync(id);
            if (tour == null) return NotFound();
            _ctx.TourPackages.Remove(tour);
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }

    public class TourPackageDto
    {
        public string NameGeo { get; set; } = "";
        public string? DescriptionGeo { get; set; }
        public decimal Price { get; set; }
        public double? DurationHours { get; set; }
        public string? ImageUrl { get; set; }
        public List<WaypointDto>? Waypoints { get; set; }
    }

    public class WaypointDto
    {
        public string Name { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
