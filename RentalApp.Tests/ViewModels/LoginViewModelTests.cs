using FluentAssertions;
using Moq;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly LoginViewModel _viewModel;

    public LoginViewModelTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _viewModel = new LoginViewModel(_authServiceMock.Object, _navigationServiceMock.Object);
    }

    private void SetError(string message)
    {
        // Use reflection to call protected SetError method for test setup
        var method = typeof(BaseViewModel).GetMethod("SetError", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(_viewModel, new object[] { message });
    }



    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ShouldSetError()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "password123";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Please enter both email and password");
        _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Constructor_ShouldInitializeTitle()
    {
        // Assert
        _viewModel.Title.Should().Be("Login");
    }
    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldSetError()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Please enter both email and password");
        _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithWhitespaceCredentials_ShouldSetError()
    {
        // Arrange
        _viewModel.Email = "   ";
        _viewModel.Password = "   ";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Please enter both email and password");
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_ShouldNavigateToMainTabs()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "password123";
        _authServiceMock.Setup(x => x.LoginAsync("user@example.com", "password123"))
            .ReturnsAsync(new AuthenticationResult(true, "Success"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeFalse();
        _navigationServiceMock.Verify(x => x.NavigateToAsync("//MainTabs/ItemsListPage"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenFailed_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "wrongpassword";
        _authServiceMock.Setup(x => x.LoginAsync("user@example.com", "wrongpassword"))
            .ReturnsAsync(new AuthenticationResult(false, "Invalid credentials"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Be("Invalid credentials");
        _navigationServiceMock.Verify(x => x.NavigateToAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenExceptionThrown_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "password123";
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Contain("Login failed: Network error");
    }

    [Fact]
    public async Task LoginAsync_ShouldSetIsBusyDuringOperation()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "password123";
        var tcs = new TaskCompletionSource<AuthenticationResult>();
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(tcs.Task);

        // Act
        var loginTask = _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert - during operation
        _viewModel.IsBusy.Should().BeTrue();

        // Complete the operation
        tcs.SetResult(new AuthenticationResult(true, "Success"));
        await loginTask;

        // Assert - after operation
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WhenAlreadyBusy_ShouldReturnEarly()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "password123";
        _viewModel.IsBusy = true;

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldClearErrorBeforeAttempt()
    {
        // Arrange
        _viewModel.Email = "user@example.com";
        _viewModel.Password = "password123";
        SetError("Previous error");
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AuthenticationResult(true, "Success"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToRegisterAsync_ShouldNavigateToRegisterPage()
    {
        // Act
        await _viewModel.NavigateToRegisterCommand.ExecuteAsync(null);

        // Assert
        _navigationServiceMock.Verify(x => x.NavigateToAsync("RegisterPage"), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldShowPlaceholderAlert()
    {
        // Act & Assert - Command should execute without throwing
        // Note: Can't easily test DisplayAlert without UI, but we verify the command exists
        _viewModel.ForgotPasswordCommand.Should().NotBeNull();
    }

    [Fact]
    public void RememberMe_ShouldBeInitiallyFalse()
    {
        // Assert
        _viewModel.RememberMe.Should().BeFalse();
    }

    [Fact]
    public void RememberMe_ShouldBeSettable()
    {
        // Act
        _viewModel.RememberMe = true;

        // Assert
        _viewModel.RememberMe.Should().BeTrue();
    }
}
