using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RentalApp.Api.Controllers;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Tests.API;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup JWT configuration
        _mockConfiguration.Setup(c => c["Jwt:Secret"]).Returns("this-is-a-test-secret-key-with-at-least-32-characters");
        _mockConfiguration.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        _controller = new AuthController(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password123!"
        };

        var createdUser = new User 
        { 
            Id = 1, 
            FirstName = request.FirstName, 
            LastName = request.LastName, 
            Email = request.Email 
        };

        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _mockUserRepository.Setup(r => r.GetDefaultRoleAsync()).ReturnsAsync((Role?)null);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@test.com",
            Password = "Password123!"
        };

        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Token_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        _mockUserRepository.Setup(r => r.GetByEmailWithRolesAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Token(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
