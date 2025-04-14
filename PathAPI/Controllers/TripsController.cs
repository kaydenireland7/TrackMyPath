using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathAPI.Models;

namespace PathAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TrackMyPathContext _context;

    public TripsController(TrackMyPathContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        => await _context.Trips.Include(t => t.User).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> GetTrip(int id)
    {
        var trip = await _context.Trips.Include(t => t.User)
                                       
                                       .Include(t => t.Photos)
                                       .FirstOrDefaultAsync(t => t.Id == id);
        return trip == null ? NotFound() : trip;
    }

    [HttpPost]
    public async Task<ActionResult<Trip>> CreateTrip(Trip trip)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, Trip trip)
    {
        if (id != trip.Id) return BadRequest();
        _context.Entry(trip).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null) return NotFound();
        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
