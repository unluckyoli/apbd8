using APBD8.Data;
using APBD8.Models;
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


    /*[HttpGet]                                 NA ZAJECIACH PRZYKLAD
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
    }*/
    
    
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Name = e.Name,
                Description = e.Description,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxPeople = e.MaxPeople,
                Countries = e.IdCountries.Select(c => new
                {
                    Name = c.Name
                }),
                Clients = e.ClientTrips.Select(ct => new
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                })
            }).ToListAsync();

        return Ok(new
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            trips
        });
    }

    
    
    
    
    
    
    
    
    
    [HttpDelete("api/clients/{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
        {
            return NotFound(new { message = "nie znaleziono klienta" });
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest(new { message = "klient ma wycieczki wiec nie mize byc usunieeety" });
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    
    
    
    [HttpPost("api/trips/{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientTripRequest request)
    {
        
        var trip = await _context.Trips
        .FirstOrDefaultAsync(t => t.IdTrip == idTrip && t.DateFrom > DateTime.Now);

        if (trip == null)
        {
            return BadRequest(new { message = "wycieczka nie istnieje lub juz sie odbyla" });
        }

        
        
        
        
        
        
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Pesel == request.Pesel);

        if (existingClient != null)
        {
            
            
            
            var existingAssignment = await _context.ClientTrips
            .FirstOrDefaultAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);

            if (existingAssignment != null)
            {
                return BadRequest(new { message = "klient jest juz w tej wyciecze" });
            }

            
            
            
            _context.ClientTrips.Add(new ClientTrip
            {
                IdClient = existingClient.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = request.PaymentDate
            });
        }
        else
        {
            
            
            
            var newClient = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Telephone = request.Telephone,
                Pesel = request.Pesel
            };

            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            
            
            
            _context.ClientTrips.Add(new ClientTrip
            {
                IdClient = newClient.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = request.PaymentDate
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "klient dodany do wycieczki" });
}
    


    
    
    
}