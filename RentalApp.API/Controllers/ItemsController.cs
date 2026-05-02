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
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _itemRepository;

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

        var response = items.Select(i => new ItemResponse
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            DailyPrice = i.DailyPrice,
            IsAvailable = i.IsAvailable,
            CreatedDate = i.CreatedDate,
            UpdatedDate = i.UpdatedDate,
            OwnerId = i.OwnerId,
            OwnerName = i.Owner.FirstName + " " + i.Owner.LastName,
            CategoryId = i.CategoryId,
            CategoryName = i.Category.Name
        }).ToList();

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

        return Ok(new ItemResponse
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
            CategoryName = item.Category.Name
        });
    }

    // GET: api/items/my
    [HttpGet("my")]
    public async Task<ActionResult<List<ItemResponse>>> GetMyItems()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var items = await _itemRepository.GetByOwnerIdAsync(userId);

        var response = items.Select(i => new ItemResponse
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            DailyPrice = i.DailyPrice,
            IsAvailable = i.IsAvailable,
            CreatedDate = i.CreatedDate,
            UpdatedDate = i.UpdatedDate,
            OwnerId = i.OwnerId,
            OwnerName = i.Owner.FirstName + " " + i.Owner.LastName,
            CategoryId = i.CategoryId,
            CategoryName = i.Category.Name
        }).ToList();

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
            CreatedDate = DateTime.UtcNow
        };

        item = await _itemRepository.CreateAsync(item);

        return CreatedAtAction(
            nameof(GetItem),
            new { id = item.Id },
            new ItemResponse
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
                CategoryName = item.Category.Name
            });
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
}
