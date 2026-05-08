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

    [Fact]
    public async Task Token_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword,
            UserRoles = new List<UserRole>()
        };

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = password
        };

        _mockUserRepository.Setup(r => r.GetByEmailWithRolesAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _controller.Token(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<TokenResponse>().Subject;
        response.Email.Should().Be(user.Email);
        response.FirstName.Should().Be(user.FirstName);
        response.LastName.Should().Be(user.LastName);
        response.Token.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Token_WithValidCredentialsAndRoles_ShouldReturnTokenWithRoles()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = hashedPassword,
            UserRoles = new List<UserRole>
            {
                new UserRole 
                { 
                    IsActive = true, 
                    Role = new Role { Name = "Admin" } 
                },
                new UserRole 
                { 
                    IsActive = true, 
                    Role = new Role { Name = "User" } 
                }
            }
        };

        var request = new LoginRequest
        {
            Email = "admin@test.com",
            Password = password
        };

        _mockUserRepository.Setup(r => r.GetByEmailWithRolesAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _controller.Token(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<TokenResponse>().Subject;
        response.Roles.Should().HaveCount(2);
        response.Roles.Should().Contain("Admin");
        response.Roles.Should().Contain("User");
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Token_WithIncorrectPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var correctPassword = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword,
            UserRoles = new List<UserRole>()
        };

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        };

        _mockUserRepository.Setup(r => r.GetByEmailWithRolesAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _controller.Token(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldHashPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password123!"
        };

        User? capturedUser = null;
        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockUserRepository.Setup(r => r.GetDefaultRoleAsync()).ReturnsAsync((Role?)null);

        // Act
        await _controller.Register(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        capturedUser.PasswordHash.Should().NotBe(request.Password); // Should be hashed
        BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithDefaultRole_ShouldAddUserRole()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password123!"
        };

        var defaultRole = new Role { Id = 1, Name = "User" };
        int capturedUserId = 0;

        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockUserRepository.Setup(r => r.GetDefaultRoleAsync()).ReturnsAsync(defaultRole);
        _mockUserRepository.Setup(r => r.AddUserRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Callback<int, int>((userId, roleId) => capturedUserId = userId)
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Register(request);

        // Assert
        _mockUserRepository.Verify(r => r.AddUserRoleAsync(It.IsAny<int>(), defaultRole.Id), Times.Once);
        capturedUserId.Should().Be(1); // Verify the correct user ID was passed
    }

    [Fact]
    public async Task Token_WithInactiveRole_ShouldNotIncludeInToken()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword,
            UserRoles = new List<UserRole>
            {
                new UserRole 
                { 
                    IsActive = true, 
                    Role = new Role { Name = "User" } 
                },
                new UserRole 
                { 
                    IsActive = false, // Inactive role 
                    Role = new Role { Name = "Admin" } 
                }
            }
        };

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = password
        };

        _mockUserRepository.Setup(r => r.GetByEmailWithRolesAsync(request.Email)).ReturnsAsync(user);

        // Act
        var result = await _controller.Token(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<TokenResponse>().Subject;
        response.Roles.Should().HaveCount(1);
        response.Roles.Should().Contain("User");
        response.Roles.Should().NotContain("Admin"); // Inactive role not included
    }

    [Fact]
    public async Task Register_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest();
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Token_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest();
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Token(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockUserRepository.Verify(r => r.GetByEmailWithRolesAsync(It.IsAny<string>()), Times.Never);
    }
}
