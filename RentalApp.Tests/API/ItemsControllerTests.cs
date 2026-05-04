using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RentalApp.Api.Controllers;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Tests.API;

public class ItemsControllerTests
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly ItemsController _controller;

    public ItemsControllerTests()
    {
        _mockItemRepository = new Mock<IItemRepository>();
        _controller = new ItemsController(_mockItemRepository.Object);

        // Setup a user context for authenticated endpoints
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetItems_ShouldReturnOkWithItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 1, Title = "Drill", DailyPrice = 10, OwnerId = 1, CategoryId = 1, 
                    Category = new Category { Id = 1, Name = "Tools" },
                    Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" } },
            new() { Id = 2, Title = "Ladder", DailyPrice = 15, OwnerId = 1, CategoryId = 1, 
                    Category = new Category { Id = 1, Name = "Tools" },
                    Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" } }
        };
        _mockItemRepository.Setup(r => r.GetAllAsync(null, null, null)).ReturnsAsync(items);

        // Act
        var result = await _controller.GetItems();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<ItemResponse>>().Subject;
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItem_WithValidId_ShouldReturnOkWithItem()
    {
        // Arrange
        var item = new Item 
        { 
            Id = 1, Title = "Drill", DailyPrice = 10, OwnerId = 1, CategoryId = 1,
            Category = new Category { Id = 1, Name = "Tools" },
            Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" }
        };
        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.GetItem(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ItemResponse>().Subject;
        response.Title.Should().Be("Drill");
    }

    [Fact]
    public async Task GetItem_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockItemRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act
        var result = await _controller.GetItem(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetMyItems_ShouldReturnOkWithUserItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 1, Title = "My Drill", DailyPrice = 10, OwnerId = 1, CategoryId = 1,
                    Category = new Category { Id = 1, Name = "Tools" },
                    Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" } }
        };
        _mockItemRepository.Setup(r => r.GetByOwnerIdAsync(1)).ReturnsAsync(items);

        // Act
        var result = await _controller.GetMyItems();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<ItemResponse>>().Subject;
        response.Should().HaveCount(1);
        response[0].Title.Should().Be("My Drill");
    }

    [Fact]
    public async Task CreateItem_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "New Drill",
            Description = "A powerful drill",
            DailyPrice = 10,
            CategoryId = 1,
            IsAvailable = true,
            Address = "123 Test St"
        };

        var createdItem = new Item
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            DailyPrice = request.DailyPrice,
            CategoryId = request.CategoryId,
            OwnerId = 1,
            IsAvailable = request.IsAvailable,
            Address = request.Address,
            Category = new Category { Id = 1, Name = "Tools" },
            Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" }
        };

        _mockItemRepository.Setup(r => r.CategoryExistsAsync(request.CategoryId)).ReturnsAsync(true);
        _mockItemRepository.Setup(r => r.CreateAsync(It.IsAny<Item>())).ReturnsAsync(createdItem);

        // Act
        var result = await _controller.CreateItem(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeAssignableTo<ItemResponse>().Subject;
        response.Title.Should().Be("New Drill");
    }

    [Fact]
    public async Task CreateItem_WithInvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateItemRequest
        {
            Title = "New Drill",
            Description = "A powerful drill",
            DailyPrice = 10,
            CategoryId = 999,
            IsAvailable = true
        };

        _mockItemRepository.Setup(r => r.CategoryExistsAsync(request.CategoryId)).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateItem(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
