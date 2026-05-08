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

public class RatingsControllerTests
{
    private readonly Mock<IRatingRepository> _mockRatingRepository;
    private readonly Mock<IRentalRepository> _mockRentalRepository;
    private readonly RatingsController _controller;

    public RatingsControllerTests()
    {
        _mockRatingRepository = new Mock<IRatingRepository>();
        _mockRentalRepository = new Mock<IRentalRepository>();
        _controller = new RatingsController(_mockRatingRepository.Object, _mockRentalRepository.Object);

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
    public async Task GetItemRatings_ShouldReturnOkWithRatings()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new() { Id = 1, RentalId = 1, RaterId = 2, RatedUserId = 1, Stars = 5, 
                    Comment = "Great item!", CreatedDate = DateTime.UtcNow,
                    Rater = new User { Id = 2, FirstName = "John", LastName = "Doe" },
                    RatedUser = new User { Id = 1, FirstName = "Jane", LastName = "Smith" } }
        };
        _mockRatingRepository.Setup(r => r.GetByItemIdAsync(1)).ReturnsAsync(ratings);

        // Act
        var result = await _controller.GetItemRatings(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<RatingResponse>>().Subject;
        response.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetItemRatingStats_ShouldReturnOkWithStats()
    {
        // Arrange
        _mockRatingRepository.Setup(r => r.GetItemRatingStatsAsync(1)).ReturnsAsync((4.5, 10));

        // Act
        var result = await _controller.GetItemRatingStats(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<RatingStatsResponse>().Subject;
        response.AverageRating.Should().Be(4.5);
        response.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task GetUserRatings_ShouldReturnOkWithRatings()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new() { Id = 1, RentalId = 1, RaterId = 2, RatedUserId = 1, Stars = 5,
                    Rater = new User { Id = 2, FirstName = "John", LastName = "Doe" },
                    RatedUser = new User { Id = 1, FirstName = "Jane", LastName = "Smith" } }
        };
        _mockRatingRepository.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(ratings);

        // Act
        var result = await _controller.GetUserRatings(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<RatingResponse>>().Subject;
        response.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserRatingStats_ShouldReturnOkWithStats()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new() { Id = 1, RatedUserId = 1, Stars = 4,
                    Rater = new User { Id = 2, FirstName = "John", LastName = "Doe" },
                    RatedUser = new User { Id = 1, FirstName = "Jane", LastName = "Smith" } },
            new() { Id = 2, RatedUserId = 1, Stars = 5,
                    Rater = new User { Id = 3, FirstName = "Jane", LastName = "Smith" },
                    RatedUser = new User { Id = 1, FirstName = "Jane", LastName = "Smith" } }
        };
        _mockRatingRepository.Setup(r => r.GetAverageRatingForUserAsync(1)).ReturnsAsync(4.5);
        _mockRatingRepository.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(ratings);

        // Act
        var result = await _controller.GetUserRatingStats(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<RatingStatsResponse>().Subject;
        response.AverageRating.Should().Be(4.5);
        response.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateRating_WithInvalidStars_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 0, // Invalid
            Comment = "Test"
        };

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Stars must be between 1 and 5");
    }

    [Fact]
    public async Task CreateRating_WithStarsTooHigh_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 6, // Invalid
            Comment = "Test"
        };

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Stars must be between 1 and 5");
    }

    [Fact]
    public async Task CreateRating_WithNonExistentRental_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 999,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Test"
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Rental not found");
    }

    [Fact]
    public async Task CreateRating_WithNonCompletedRental_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Test"
        };
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1,
            Status = "Active", // Not completed
            Item = new Item { OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Can only rate completed rentals");
    }

    [Fact]
    public async Task CreateRating_WhenNotRenter_ShouldReturnForbid()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Test"
        };
        var rental = new Rental
        {
            Id = 1,
            RenterId = 999, // Different user
            Status = "Completed",
            Item = new Item { OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CreateRating_WhenRatingAlreadyExists_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Test"
        };
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1,
            Status = "Completed",
            Item = new Item { OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.RatingExistsForRentalAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Rating already exists for this rental");
    }

    [Fact]
    public async Task CreateRating_WithWrongRatedUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 999, // Wrong user
            Stars = 5,
            Comment = "Test"
        };
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1,
            Status = "Completed",
            Item = new Item { OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.RatingExistsForRentalAsync(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Can only rate the item owner");
    }

    [Fact]
    public async Task CreateRating_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateRatingRequest
        {
            RentalId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Great rental experience!"
        };
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1,
            Status = "Completed",
            Item = new Item { OwnerId = 2 }
        };
        var createdRating = new Rating
        {
            Id = 100,
            RentalId = 1,
            RaterId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Great rental experience!",
            CreatedDate = DateTime.UtcNow,
            Rater = new User { Id = 1, FirstName = "John", LastName = "Doe" },
            RatedUser = new User { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.RatingExistsForRentalAsync(1)).ReturnsAsync(false);
        _mockRatingRepository.Setup(r => r.CreateAsync(It.IsAny<Rating>())).ReturnsAsync(createdRating);

        // Act
        var result = await _controller.CreateRating(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetRentalRating");
        var response = createdResult.Value.Should().BeAssignableTo<RatingResponse>().Subject;
        response.Stars.Should().Be(5);
        response.Comment.Should().Be("Great rental experience!");
    }

    [Fact]
    public async Task GetRentalRating_WithNonExistentRental_ShouldReturnNotFound()
    {
        // Arrange
        _mockRentalRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Rental?)null);

        // Act
        var result = await _controller.GetRentalRating(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Rental not found");
    }

    [Fact]
    public async Task GetRentalRating_WhenUserNotParticipant_ShouldReturnForbid()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            RenterId = 999, // Different user
            Item = new Item { OwnerId = 888 } // Different owner
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act
        var result = await _controller.GetRentalRating(1);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetRentalRating_WithNoRating_ShouldReturnNotFound()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1, // Current user is renter
            Item = new Item { OwnerId = 2 }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.GetByRentalIdAsync(1)).ReturnsAsync((Rating?)null);

        // Act
        var result = await _controller.GetRentalRating(1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("No rating found for this rental");
    }

    [Fact]
    public async Task GetRentalRating_WhenUserIsRenter_ShouldReturnOk()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            RenterId = 1, // Current user
            Item = new Item { OwnerId = 2 }
        };
        var rating = new Rating
        {
            Id = 100,
            RentalId = 1,
            RaterId = 1,
            RatedUserId = 2,
            Stars = 5,
            Comment = "Great!",
            Rater = new User { Id = 1, FirstName = "John", LastName = "Doe" },
            RatedUser = new User { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.GetByRentalIdAsync(1)).ReturnsAsync(rating);

        // Act
        var result = await _controller.GetRentalRating(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<RatingResponse>().Subject;
        response.Stars.Should().Be(5);
        response.Comment.Should().Be("Great!");
    }

    [Fact]
    public async Task GetRentalRating_WhenUserIsOwner_ShouldReturnOk()
    {
        // Arrange
        var rental = new Rental
        {
            Id = 1,
            RenterId = 2,
            Item = new Item { OwnerId = 1 } // Current user is owner
        };
        var rating = new Rating
        {
            Id = 100,
            RentalId = 1,
            RaterId = 2,
            RatedUserId = 1,
            Stars = 4,
            Comment = "Good owner!",
            Rater = new User { Id = 2, FirstName = "Jane", LastName = "Smith" },
            RatedUser = new User { Id = 1, FirstName = "John", LastName = "Doe" }
        };
        _mockRentalRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _mockRatingRepository.Setup(r => r.GetByRentalIdAsync(1)).ReturnsAsync(rating);

        // Act
        var result = await _controller.GetRentalRating(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<RatingResponse>().Subject;
        response.Stars.Should().Be(4);
        response.Comment.Should().Be("Good owner!");
    }
}
