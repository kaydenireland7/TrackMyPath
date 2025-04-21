using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathAPI.Models;

namespace PathAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly TrackMyPathContext _context;

    public PhotosController(TrackMyPathContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        => await _context.Photos/*.Include(p => p.Location)*/.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Photo>> GetPhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        return photo == null ? NotFound() : photo;
    }

    [HttpPost]
    public async Task<ActionResult<Photo>> CreatePhoto(Photo photo)
    {
        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPhoto), new { id = photo.Id }, photo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhoto(int id, Photo photo)
    {
        if (id != photo.Id) return BadRequest();
        _context.Entry(photo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null) return NotFound();
        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
