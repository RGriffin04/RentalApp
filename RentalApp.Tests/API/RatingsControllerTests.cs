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
}
