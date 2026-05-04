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
}
