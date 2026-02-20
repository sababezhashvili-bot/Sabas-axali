using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;

namespace Racha629.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET ALL AD SPACES (Public)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdSpace>>> GetAdSpaces()
        {
            return await _context.AdSpaces.ToListAsync();
        }

        // 2. CREATE AD SPACE (Admin Only)
        [HttpPost]
        public async Task<ActionResult<AdSpace>> CreateAdSpace(AdSpace adSpace)
        {
            // In a real app, authorize Admin here
            _context.AdSpaces.Add(adSpace);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAdSpaces), new { id = adSpace.Id }, adSpace);
        }

        // 3. RENT REQUEST (User)
        [HttpPost("{id}/rent")]
        public async Task<IActionResult> RentAdSpace(int id, [FromBody] RentRequest request)
        {
            var adSpace = await _context.AdSpaces.FindAsync(id);
            if (adSpace == null) return NotFound("Ad Space not found");

            // Allow requesting if Available OR Pending (queueing)
            // But visual specs say: Green(Available) -> Orange(Pending) -> Red(Rented)
            
            request.AdSpaceId = id;
            request.Status = "Pending";
            request.RequestDate = DateTime.UtcNow;

            _context.RentRequests.Add(request);
            
            // Update visual status to Pending immediately
            if (adSpace.Status == "Available")
            {
                adSpace.Status = "Pending";
            }

            await _context.SaveChangesAsync();
            return Ok("Rent request submitted");
        }

        // 4. MANAGE REQUESTS (Admin: Approve/Reject)
        [HttpPut("requests/{id}/{status}")]
        public async Task<IActionResult> ManageRequest(int id, string status)
        {
            var request = await _context.RentRequests.FindAsync(id);
            if (request == null) return NotFound("Request not found");

            var adSpace = await _context.AdSpaces.FindAsync(request.AdSpaceId);
            if (adSpace == null) return NotFound("Associated Ad Space not found");

            if (status.ToLower() == "approve")
            {
                request.Status = "Approved";
                
                // Update Ad Space
                adSpace.Status = "Rented";
                adSpace.CurrentRenterId = request.UserId;
                adSpace.CurrentImageUrl = request.ImageUrl; // Update the billboard image!
                
                // Reject other pending requests for this space
                var otherRequests = await _context.RentRequests
                    .Where(r => r.AdSpaceId == adSpace.Id && r.Id != request.Id && r.Status == "Pending")
                    .ToListAsync();
                
                foreach(var r in otherRequests) r.Status = "Rejected";
            }
            else if (status.ToLower() == "reject")
            {
                request.Status = "Rejected";
                
                // If no other pending requests, revert to Available
                var anyOtherPending = await _context.RentRequests
                    .AnyAsync(r => r.AdSpaceId == adSpace.Id && r.Id != request.Id && r.Status == "Pending");
                
                if (!anyOtherPending && adSpace.Status == "Pending") 
                {
                     adSpace.Status = "Available";
                }
            }
            else 
            {
                return BadRequest("Invalid status. Use 'approve' or 'reject'");
            }

            await _context.SaveChangesAsync();
            return Ok($"Request {status}");
        }
        
         // 5. GET REQUESTS (Admin)
        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<RentRequest>>> GetRequests()
        {
            return await _context.RentRequests.ToListAsync();
        }
    }
}
