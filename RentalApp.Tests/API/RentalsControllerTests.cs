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

public class RentalsControllerTests
{
    private readonly Mock<IRentalRepository> _mockRentalRepository;
    private readonly RentalsController _controller;

    public RentalsControllerTests()
    {
        _mockRentalRepository = new Mock<IRentalRepository>();
        _controller = new RentalsController(_mockRentalRepository.Object);

        // Setup user context
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
    public async Task GetMyRentals_ShouldReturnOkWithRentals()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new() 
            { 
                Id = 1, ItemId = 1, RenterId = 1, Status = "Pending",
                TotalPrice = 100, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5),
                Item = new Item 
                { 
                    Id = 1, Title = "Drill", OwnerId = 2, CategoryId = 1,
                    Owner = new User { Id = 2, FirstName = "Jane", LastName = "Owner" }
                },
                Renter = new User { Id = 1, FirstName = "John", LastName = "Renter" }
            }
        };
        _mockRentalRepository.Setup(r => r.GetByRenterIdAsync(1, null)).ReturnsAsync(rentals);

        // Act
        var result = await _controller.GetMyRentals();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<RentalResponse>>().Subject;
        response.Should().HaveCount(1);
        response[0].ItemTitle.Should().Be("Drill");
    }

    [Fact]
    public async Task GetMyRentals_WithStatusFilter_ShouldReturnFilteredRentals()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new() 
            { 
                Id = 1, ItemId = 1, RenterId = 1, Status = "Approved",
                TotalPrice = 100, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5),
                Item = new Item 
                { 
                    Id = 1, Title = "Drill", OwnerId = 2, CategoryId = 1,
                    Owner = new User { Id = 2, FirstName = "Jane", LastName = "Owner" }
                },
                Renter = new User { Id = 1, FirstName = "John", LastName = "Renter" }
            }
        };
        _mockRentalRepository.Setup(r => r.GetByRenterIdAsync(1, "Approved")).ReturnsAsync(rentals);

        // Act
        var result = await _controller.GetMyRentals("Approved");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<RentalResponse>>().Subject;
        response.Should().HaveCount(1);
        response[0].Status.Should().Be("Approved");
    }

    [Fact]
    public async Task GetRentalsAsOwner_ShouldReturnOkWithRentals()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new() 
            { 
                Id = 1, ItemId = 1, RenterId = 2, Status = "Pending",
                TotalPrice = 100, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5),
                Item = new Item 
                { 
                    Id = 1, Title = "Ladder", OwnerId = 1, CategoryId = 1,
                    Owner = new User { Id = 1, FirstName = "John", LastName = "Owner" }
                },
                Renter = new User { Id = 2, FirstName = "Jane", LastName = "Renter" }
            }
        };
        _mockRentalRepository.Setup(r => r.GetByOwnerIdAsync(1, null)).ReturnsAsync(rentals);

        // Act
        var result = await _controller.GetRentalsAsOwner();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<RentalResponse>>().Subject;
        response.Should().HaveCount(1);
        response[0].ItemTitle.Should().Be("Ladder");
    }

    [Fact]
    public async Task GetRental_WithValidId_ShouldReturnOkWithRental()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1, ItemId = 1, RenterId = 1, Status = "Approved",
            TotalPrice = 100, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5),
            Item = new Item 
            { 
                Id = 1, Title = "Drill", OwnerId = 2, CategoryId = 1,
                Owner = new User { Id = 2, FirstName = "Jane", LastName = "Owner" }
            },
            Renter = new User { Id = 1, FirstName = "John", LastName = "Renter" }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.GetRental(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<RentalResponse>().Subject;
        response.ItemTitle.Should().Be("Drill");
    }

    [Fact]
    public async Task GetRental_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.GetRental(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRental_WhenUserIsNotAuthorized_ShouldReturnForbid()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1, ItemId = 1, RenterId = 99, Status = "Approved",
            Item = new Item { Id = 1, OwnerId = 88, CategoryId = 1 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.GetRental(1);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CreateRental_WithItemNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 999,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(999)).ReturnsAsync((Item?)null);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Item not found");
    }

    [Fact]
    public async Task CreateRental_WithUnavailableItem_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = false, // Item not available
            OwnerId = 2
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Item is not available");
    }

    [Fact]
    public async Task CreateRental_WhenRentingOwnItem_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 1 // Same as current user
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("You cannot rent your own item");
    }

    [Fact]
    public async Task CreateRental_WithPastStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(-1), // Past date
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 2
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Start date cannot be in the past");
    }

    [Fact]
    public async Task CreateRental_WithEndDateBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(1), // Before start date
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 2
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("End date must be after start date");
    }

    [Fact]
    public async Task CreateRental_WithOverlappingRentals_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 2
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);
        _mockRentalRepository.Setup(r => r.HasOverlappingRentalsAsync(
            1, 
            It.IsAny<DateTime>(), 
            It.IsAny<DateTime>()))
            .ReturnsAsync(true); // Has overlap

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Item is already rented for the selected dates");
    }

    [Fact]
    public async Task CreateRental_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 2,
            Owner = new User { Id = 2, FirstName = "Jane", LastName = "Owner" }
        };
        var createdRental = new Rental
        {
            Id = 100,
            ItemId = 1,
            RenterId = 1,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            TotalPrice = 100,
            Status = "Pending",
            CreatedDate = DateTime.UtcNow,
            Item = item,
            Renter = new User { Id = 1, FirstName = "John", LastName = "Doe" }
        };

        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);
        _mockRentalRepository.Setup(r => r.HasOverlappingRentalsAsync(
            1, 
            It.IsAny<DateTime>(), 
            It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _mockRentalRepository.Setup(r => r.CreateAsync(It.IsAny<Rental>())).ReturnsAsync(createdRental);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetRental");
        var response = createdResult.Value.Should().BeAssignableTo<RentalResponse>().Subject;
        response.Status.Should().Be("Pending");
        response.TotalPrice.Should().Be(100);
    }

    [Fact]
    public async Task CancelRental_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 1, // Current user
            Status = "Pending",
            Item = new Item { Id = 1, OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRentalRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CancelRental(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRentalRepository.Verify(r => r.UpdateAsync(It.Is<Rental>(
            r => r.Status == "Cancelled")), Times.Once);
    }

    [Fact]
    public async Task CancelRental_WithNonExistentRental_ShouldReturnNotFound()
    {
        // Arrange
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.CancelRental(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CancelRental_WhenUnauthorized_ShouldReturnForbid()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 999, // Different user
            Status = "Pending",
            Item = new Item { Id = 1, OwnerId = 888 } // Different owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CancelRental(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CancelRental_WithNonPendingStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 1,
            Status = "Completed", // Not pending
            Item = new Item { Id = 1, OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CancelRental(1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Only pending rentals can be cancelled");
    }

    [Fact]
    public async Task ApproveRental_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Pending",
            Item = new Item { Id = 1, OwnerId = 1 } // Current user is owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRentalRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ApproveRental(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRentalRepository.Verify(r => r.UpdateAsync(It.Is<Rental>(
            r => r.Status == "Active")), Times.Once);
    }

    [Fact]
    public async Task ApproveRental_WithNonExistentRental_ShouldReturnNotFound()
    {
        // Arrange
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.ApproveRental(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ApproveRental_WhenNotOwner_ShouldReturnForbid()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Pending",
            Item = new Item { Id = 1, OwnerId = 999 } // Different owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.ApproveRental(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task ApproveRental_WithNonPendingStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Active", // Not pending
            Item = new Item { Id = 1, OwnerId = 1 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.ApproveRental(1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Only pending rentals can be approved");
    }

    [Fact]
    public async Task CompleteRental_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Active",
            Item = new Item { Id = 1, OwnerId = 1 } // Current user is owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRentalRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteRental(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRentalRepository.Verify(r => r.UpdateAsync(It.Is<Rental>(
            r => r.Status == "Completed" && r.CompletedDate != null)), Times.Once);
    }

    [Fact]
    public async Task CompleteRental_WithNonExistentRental_ShouldReturnNotFound()
    {
        // Arrange
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.CompleteRental(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CompleteRental_WhenNotOwner_ShouldReturnForbid()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Active",
            Item = new Item { Id = 1, OwnerId = 999 } // Different owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CompleteRental(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CompleteRental_WithNonActiveStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 2,
            Status = "Pending", // Not active
            Item = new Item { Id = 1, OwnerId = 1 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CompleteRental(1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Only active rentals can be completed");
    }

    [Fact]
    public async Task CreateRental_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest();
        _controller.ModelState.AddModelError("ItemId", "ItemId is required");

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CancelRental_AsOwner_ShouldReturnNoContent()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            ItemId = 1,
            RenterId = 999, // Different user
            Status = "Pending",
            Item = new Item { Id = 1, OwnerId = 1 } // Current user is owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRentalRepository.Setup(r => r.UpdateAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CancelRental(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CreateRental_WithEqualStartAndEndDate_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRentalRequest
        {
            ItemId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(5), // Equal to start date
            TotalPrice = 100
        };
        var item = new Item
        {
            Id = 1,
            Title = "Drill",
            IsAvailable = true,
            OwnerId = 2
        };
        _mockRentalRepository.Setup(r => r.GetItemWithOwnerAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.CreateRental(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("End date must be after start date");
    }
}
