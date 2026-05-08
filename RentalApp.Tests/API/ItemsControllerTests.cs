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

    [Fact]
    public async Task UpdateItem_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Title = "Old Title",
            Description = "Old Description",
            DailyPrice = 10,
            OwnerId = 1,
            CategoryId = 1,
            IsAvailable = true
        };
        var request = new UpdateItemRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            DailyPrice = 15
        };

        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _mockItemRepository.Setup(r => r.UpdateAsync(It.IsAny<Item>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateItem(1, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockItemRepository.Verify(r => r.UpdateAsync(It.Is<Item>(i =>
            i.Title == "Updated Title" &&
            i.Description == "Updated Description" &&
            i.DailyPrice == 15)), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_WithNonExistentItem_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateItemRequest { Title = "Updated" };
        _mockItemRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act
        var result = await _controller.UpdateItem(999, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateItem_WhenNotOwner_ShouldReturnForbid()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Title = "Title",
            OwnerId = 999 // Different owner
        };
        var request = new UpdateItemRequest { Title = "Updated" };
        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.UpdateItem(1, request);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdateItem_WithInvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Title = "Title",
            OwnerId = 1,
            CategoryId = 1
        };
        var request = new UpdateItemRequest { CategoryId = 999 };
        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _mockItemRepository.Setup(r => r.CategoryExistsAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateItem(1, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Invalid category");
    }

    [Fact]
    public async Task DeleteItem_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Title = "Title",
            OwnerId = 1
        };
        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _mockItemRepository.Setup(r => r.DeleteAsync(item)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockItemRepository.Verify(r => r.DeleteAsync(item), Times.Once);
    }

    [Fact]
    public async Task DeleteItem_WithNonExistentItem_ShouldReturnNotFound()
    {
        // Arrange
        _mockItemRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act
        var result = await _controller.DeleteItem(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteItem_WhenNotOwner_ShouldReturnForbid()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Title = "Title",
            OwnerId = 999 // Different owner
        };
        _mockItemRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetNearbyItems_WithValidCoordinates_ShouldReturnOkWithItems()
    {
        // Arrange
        var geometryFactory = new NetTopologySuite.Geometries.GeometryFactory(
            new NetTopologySuite.Geometries.PrecisionModel(), 4326);

        var itemsWithDistance = new List<(Item Item, double DistanceKm)>
        {
            (new Item
            {
                Id = 1,
                Title = "Nearby Drill",
                DailyPrice = 10,
                OwnerId = 1,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Tools" },
                Owner = new User { Id = 1, FirstName = "John", LastName = "Doe" },
                Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(-74.0060, 40.7128))
            }, 2.5)
        };
        _mockItemRepository.Setup(r => r.GetNearbyWithDistanceAsync(40.7128, -74.0060, 10.0))
            .ReturnsAsync(itemsWithDistance);

        // Act
        var result = await _controller.GetNearbyItems(40.7128, -74.0060, 10.0);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<ItemWithDistanceResponse>>().Subject;
        response.Should().HaveCount(1);
        response[0].DistanceKm.Should().Be(2.5);
    }

    [Fact]
    public async Task GetNearbyItems_WithInvalidLatitude_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetNearbyItems(95.0, -74.0060); // Invalid lat

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Latitude must be between -90 and 90");
    }

    [Fact]
    public async Task GetNearbyItems_WithInvalidLongitude_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetNearbyItems(40.7128, 200.0); // Invalid lon

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Longitude must be between -180 and 180");
    }

    [Fact]
    public async Task GetNearbyItems_WithInvalidRadius_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetNearbyItems(40.7128, -74.0060, 150.0); // Invalid radius

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Radius must be between 0 and 100 km");
    }
}
