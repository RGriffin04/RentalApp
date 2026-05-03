using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalRepository _rentalRepository;

    public RentalsController(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    // GET: api/rentals/my
    [HttpGet("my")]
    public async Task<ActionResult<List<RentalResponse>>> GetMyRentals([FromQuery] string? status = null)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rentals = await _rentalRepository.GetByRenterIdAsync(userId, status);

        var response = rentals.Select(r => new RentalResponse
        {
            Id = r.Id,
            ItemId = r.ItemId,
            ItemTitle = r.Item.Title,
            RenterId = r.RenterId,
            RenterName = r.Renter.FirstName + " " + r.Renter.LastName,
            OwnerId = r.Item.OwnerId,
            OwnerName = r.Item.Owner.FirstName + " " + r.Item.Owner.LastName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            CreatedDate = r.CreatedDate,
            CompletedDate = r.CompletedDate
        }).ToList();

        return Ok(response);
    }

    // GET: api/rentals/as-owner
    [HttpGet("as-owner")]
    public async Task<ActionResult<List<RentalResponse>>> GetRentalsAsOwner([FromQuery] string? status = null)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rentals = await _rentalRepository.GetByOwnerIdAsync(userId, status);

        var response = rentals.Select(r => new RentalResponse
        {
            Id = r.Id,
            ItemId = r.ItemId,
            ItemTitle = r.Item.Title,
            RenterId = r.RenterId,
            RenterName = r.Renter.FirstName + " " + r.Renter.LastName,
            OwnerId = r.Item.OwnerId,
            OwnerName = r.Item.Owner.FirstName + " " + r.Item.Owner.LastName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            CreatedDate = r.CreatedDate,
            CompletedDate = r.CompletedDate
        }).ToList();

        return Ok(response);
    }

    // GET: api/rentals/5
    [HttpGet("{id}")]
    public async Task<ActionResult<RentalResponse>> GetRental(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rental = await _rentalRepository.GetByIdAsync(id);

        if (rental == null)
            return NotFound();

        // User must be either the renter or the owner
        if (rental.RenterId != userId && rental.Item.OwnerId != userId)
            return Forbid();

        return Ok(new RentalResponse
        {
            Id = rental.Id,
            ItemId = rental.ItemId,
            ItemTitle = rental.Item.Title,
            RenterId = rental.RenterId,
            RenterName = rental.Renter.FirstName + " " + rental.Renter.LastName,
            OwnerId = rental.Item.OwnerId,
            OwnerName = rental.Item.Owner.FirstName + " " + rental.Item.Owner.LastName,
            StartDate = rental.StartDate,
            EndDate = rental.EndDate,
            TotalPrice = rental.TotalPrice,
            Status = rental.Status,
            CreatedDate = rental.CreatedDate,
            CompletedDate = rental.CompletedDate
        });
    }

    // POST: api/rentals
    [HttpPost]
    public async Task<ActionResult<RentalResponse>> CreateRental([FromBody] CreateRentalRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Convert dates to UTC to ensure compatibility with PostgreSQL
        var startDateUtc = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

        // Validate item exists and is available
        var item = await _rentalRepository.GetItemWithOwnerAsync(request.ItemId);

        if (item == null)
            return NotFound("Item not found");

        if (!item.IsAvailable)
            return BadRequest("Item is not available");

        if (item.OwnerId == userId)
            return BadRequest("You cannot rent your own item");

        // Validate dates
        if (startDateUtc < DateTime.UtcNow.Date)
            return BadRequest("Start date cannot be in the past");

        if (endDateUtc <= startDateUtc)
            return BadRequest("End date must be after start date");

        // Check for overlapping rentals
        var hasOverlap = await _rentalRepository.HasOverlappingRentalsAsync(
            request.ItemId, startDateUtc, endDateUtc);

        if (hasOverlap)
            return BadRequest("Item is already rented for the selected dates");

        var rental = new Rental
        {
            ItemId = request.ItemId,
            RenterId = userId,
            StartDate = startDateUtc,
            EndDate = endDateUtc,
            TotalPrice = request.TotalPrice,
            Status = "Pending",
            CreatedDate = DateTime.UtcNow
        };

        rental = await _rentalRepository.CreateAsync(rental);

        return CreatedAtAction(
            nameof(GetRental),
            new { id = rental.Id },
            new RentalResponse
            {
                Id = rental.Id,
                ItemId = rental.ItemId,
                ItemTitle = rental.Item.Title,
                RenterId = rental.RenterId,
                RenterName = rental.Renter.FirstName + " " + rental.Renter.LastName,
                OwnerId = rental.Item.OwnerId,
                OwnerName = rental.Item.Owner.FirstName + " " + rental.Item.Owner.LastName,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                TotalPrice = rental.TotalPrice,
                Status = rental.Status,
                CreatedDate = rental.CreatedDate,
                CompletedDate = rental.CompletedDate
            });
    }

    // PUT: api/rentals/5/cancel
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult> CancelRental(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rental = await _rentalRepository.GetByIdAsync(id);

        if (rental == null)
            return NotFound();

        // Only renter or owner can cancel
        if (rental.RenterId != userId && rental.Item.OwnerId != userId)
            return Forbid();

        if (rental.Status != "Pending")
            return BadRequest("Only pending rentals can be cancelled");

        rental.Status = "Cancelled";
        await _rentalRepository.UpdateAsync(rental);

        return NoContent();
    }

    // PUT: api/rentals/5/approve
    [HttpPut("{id}/approve")]
    public async Task<ActionResult> ApproveRental(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rental = await _rentalRepository.GetByIdAsync(id);

        if (rental == null)
            return NotFound();

        // Only owner can approve
        if (rental.Item.OwnerId != userId)
            return Forbid();

        if (rental.Status != "Pending")
            return BadRequest("Only pending rentals can be approved");

        rental.Status = "Active";
        await _rentalRepository.UpdateAsync(rental);

        return NoContent();
    }

    // PUT: api/rentals/5/complete
    [HttpPut("{id}/complete")]
    public async Task<ActionResult> CompleteRental(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rental = await _rentalRepository.GetByIdAsync(id);

        if (rental == null)
            return NotFound();

        // Only owner can complete
        if (rental.Item.OwnerId != userId)
            return Forbid();

        if (rental.Status != "Active")
            return BadRequest("Only active rentals can be completed");

        rental.Status = "Completed";
        rental.CompletedDate = DateTime.UtcNow;
        await _rentalRepository.UpdateAsync(rental);

        return NoContent();
    }
}
