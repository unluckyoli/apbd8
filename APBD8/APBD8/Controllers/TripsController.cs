using APBD8.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{


    private readonly Apbd8Context _context;

    public TripsController(Apbd8Context context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips =await _context.Trips.Select(e=>new
        {
            Name = e.Name,
            Countries = e.IdCountries.Select(c => new
            {
                Name =c.Name
            })
        }).ToListAsync();
        
        return Ok(trips);
    }
    
    
    
}