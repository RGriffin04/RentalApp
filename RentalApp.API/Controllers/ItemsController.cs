using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _itemRepository;
    private static readonly GeometryFactory _geometryFactory = 
        new GeometryFactory(new PrecisionModel(), 4326);

    public ItemsController(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    // GET: api/items
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemResponse>>> GetItems(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isAvailable = null)
    {
        var items = await _itemRepository.GetAllAsync(search, categoryId, isAvailable);
        var response = items.Select(MapToItemResponse).ToList();
        return Ok(response);
    }

    // GET: api/items/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemResponse>> GetItem(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound();

        return Ok(MapToItemResponse(item));
    }

    // GET: api/items/my
    [HttpGet("my")]
    public async Task<ActionResult<List<ItemResponse>>> GetMyItems()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _itemRepository.GetByOwnerIdAsync(userId);
        var response = items.Select(MapToItemResponse).ToList();
        return Ok(response);
    }

    // POST: api/items
    [HttpPost]
    public async Task<ActionResult<ItemResponse>> CreateItem([FromBody] CreateItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Validate category exists
        if (!await _itemRepository.CategoryExistsAsync(request.CategoryId))
            return BadRequest("Invalid category");

        var item = new Item
        {
            Title = request.Title,
            Description = request.Description,
            DailyPrice = request.DailyPrice,
            CategoryId = request.CategoryId,
            OwnerId = userId,
            IsAvailable = request.IsAvailable,
            CreatedDate = DateTime.UtcNow,
            Address = request.Address
        };

        // Set location if coordinates provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            item.Location = _geometryFactory.CreatePoint(
                new Coordinate(request.Longitude.Value, request.Latitude.Value));
        }

        item = await _itemRepository.CreateAsync(item);

        return CreatedAtAction(
            nameof(GetItem),
            new { id = item.Id },
            MapToItemResponse(item));
    }

    // PUT: api/items/5
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItem(int id, [FromBody] UpdateItemRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _itemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound();

        if (item.OwnerId != userId)
            return Forbid();

        if (request.Title != null)
            item.Title = request.Title;

        if (request.Description != null)
            item.Description = request.Description;

        if (request.DailyPrice.HasValue)
            item.DailyPrice = request.DailyPrice.Value;

        if (request.CategoryId.HasValue)
        {
            if (!await _itemRepository.CategoryExistsAsync(request.CategoryId.Value))
                return BadRequest("Invalid category");
            item.CategoryId = request.CategoryId.Value;
        }

        if (request.IsAvailable.HasValue)
            item.IsAvailable = request.IsAvailable.Value;

        // Update location if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            item.Location = _geometryFactory.CreatePoint(
                new Coordinate(request.Longitude.Value, request.Latitude.Value));
        }

        if (request.Address != null)
            item.Address = request.Address;

        item.UpdatedDate = DateTime.UtcNow;

        await _itemRepository.UpdateAsync(item);

        return NoContent();
    }

    // DELETE: api/items/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItem(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _itemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound();

        if (item.OwnerId != userId)
            return Forbid();

        await _itemRepository.DeleteAsync(item);

        return NoContent();
    }

    // GET: api/items/nearby?lat={lat}&lon={lon}&radius={radius}
    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemWithDistanceResponse>>> GetNearbyItems(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radius = 10.0) // Default 10km radius
    {
        if (lat < -90 || lat > 90)
            return BadRequest("Latitude must be between -90 and 90");

        if (lon < -180 || lon > 180)
            return BadRequest("Longitude must be between -180 and 180");

        if (radius <= 0 || radius > 100)
            return BadRequest("Radius must be between 0 and 100 km");

        var itemsWithDistance = await _itemRepository.GetNearbyWithDistanceAsync(lat, lon, radius);

        var response = itemsWithDistance.Select(x => new ItemWithDistanceResponse
        {
            Id = x.Item.Id,
            Title = x.Item.Title,
            Description = x.Item.Description,
            DailyPrice = x.Item.DailyPrice,
            IsAvailable = x.Item.IsAvailable,
            CreatedDate = x.Item.CreatedDate,
            UpdatedDate = x.Item.UpdatedDate,
            OwnerId = x.Item.OwnerId,
            OwnerName = x.Item.Owner.FirstName + " " + x.Item.Owner.LastName,
            CategoryId = x.Item.CategoryId,
            CategoryName = x.Item.Category.Name,
            Latitude = x.Item.Latitude,
            Longitude = x.Item.Longitude,
            Address = x.Item.Address,
            DistanceKm = x.DistanceKm
        }).ToList();

        return Ok(response);
    }

    // Helper method to map Item to ItemResponse
    private static ItemResponse MapToItemResponse(Item item)
    {
        return new ItemResponse
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            DailyPrice = item.DailyPrice,
            IsAvailable = item.IsAvailable,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate,
            OwnerId = item.OwnerId,
            OwnerName = item.Owner.FirstName + " " + item.Owner.LastName,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            Address = item.Address
        };
    }
}
