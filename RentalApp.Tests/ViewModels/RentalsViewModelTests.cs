using FluentAssertions;
using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Tests.ViewModels;

/// <summary>
/// Tests for RentalsViewModel - demonstrates a testing pattern for ViewModels
/// </summary>
public class RentalsViewModelTests
{
    // Mock dependencies
    private readonly Mock<IRentalService> _rentalServiceMock;
    private readonly Mock<IRatingService> _ratingServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;

    // System under test
    private readonly RentalsViewModel _viewModel;

    public RentalsViewModelTests()
    {
        // Initialize mocks
        _rentalServiceMock = new Mock<IRentalService>();
        _ratingServiceMock = new Mock<IRatingService>();
        _navigationServiceMock = new Mock<INavigationService>();

        // Create the ViewModel with mocked dependencies
        _viewModel = new RentalsViewModel(
            _rentalServiceMock.Object,
            _ratingServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public async Task LoadDataAsync_WhenCalled_LoadsMyRentals()
    {
        // Arrange
        var expectedRentals = new List<Rental>
        {
            new Rental 
            { 
                Id = 1, 
                ItemId = 1,
                RenterId = 100,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Status = "Pending"
            },
            new Rental 
            { 
                Id = 2, 
                ItemId = 2,
                RenterId = 100,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(10),
                Status = "Approved"
            }
        };

        _rentalServiceMock
            .Setup(x => x.GetMyRentalsAsync())
            .ReturnsAsync(expectedRentals);

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.MyRentals.Should().HaveCount(2);
        _viewModel.MyRentals.Should().BeEquivalentTo(expectedRentals);
        _viewModel.IsBusy.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDataAsync_WhenServiceFails_SetsError()
    {
        // Arrange
        var errorMessage = "Network error";
        _rentalServiceMock
            .Setup(x => x.GetMyRentalsAsync())
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Contain(errorMessage);
        _viewModel.MyRentals.Should().BeEmpty();
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDataAsync_WhenAlreadyBusy_DoesNotLoadAgain()
    {
        // Arrange
        var slowTask = new TaskCompletionSource<List<Rental>>();
        _rentalServiceMock
            .Setup(x => x.GetMyRentalsAsync())
            .Returns(slowTask.Task);

        // Act - start first load
        var firstLoad = _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Verify we're busy
        _viewModel.IsBusy.Should().BeTrue();

        // Try to load again while busy
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert - service should only be called once
        _rentalServiceMock.Verify(x => x.GetMyRentalsAsync(), Times.Once);

        // Cleanup
        slowTask.SetResult(new List<Rental>());
        await firstLoad;
    }

    [Fact]
    public async Task RefreshAsync_WhenCalled_ReloadsData()
    {
        // Arrange
        var expectedRentals = new List<Rental>
        {
            new Rental { Id = 1, ItemId = 1, RenterId = 100, Status = "Approved" }
        };

        _rentalServiceMock
            .Setup(x => x.GetMyRentalsAsync())
            .ReturnsAsync(expectedRentals);

        // Act
        await _viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        _viewModel.MyRentals.Should().HaveCount(1);
        _viewModel.IsRefreshing.Should().BeFalse();
        _rentalServiceMock.Verify(x => x.GetMyRentalsAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelRentalAsync_WhenSuccessful_ReloadsData()
    {
        // Arrange
        var rental = new Rental 
        { 
            Id = 1, 
            ItemId = 1,
            RenterId = 100,
            Status = "Pending" 
        };

        _viewModel.MyRentals.Add(rental);

        _rentalServiceMock
            .Setup(x => x.CancelRentalAsync(rental.Id))
            .ReturnsAsync(true);

        _rentalServiceMock
            .Setup(x => x.GetMyRentalsAsync())
            .ReturnsAsync(new List<Rental>());

        // Act
        await _viewModel.CancelRentalCommand.ExecuteAsync(rental);

        // Assert
        _rentalServiceMock.Verify(x => x.CancelRentalAsync(rental.Id), Times.Once);
        _rentalServiceMock.Verify(x => x.GetMyRentalsAsync(), Times.Once);
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task CancelRentalAsync_WhenFails_SetsError()
    {
        // Arrange
        var rental = new Rental { Id = 1, ItemId = 1, RenterId = 100 };
        var errorMessage = "Cannot cancel approved rental";

        _rentalServiceMock
            .Setup(x => x.CancelRentalAsync(rental.Id))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        await _viewModel.CancelRentalCommand.ExecuteAsync(rental);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task LeaveReviewAsync_NavigatesToReviewPage()
    {
        // Arrange
        var rental = new Rental 
        { 
            Id = 1, 
            ItemId = 5,
            RenterId = 100,
            OwnerId = 200,
            Status = "Completed" 
        };

        // Act
        await _viewModel.LeaveReviewCommand.ExecuteAsync(rental);

        // Assert
        _navigationServiceMock.Verify(
            x => x.NavigateToAsync(It.Is<string>(s => s.Contains("CreateReviewPage") && s.Contains($"rentalId={rental.Id}"))), 
            Times.Once);
    }

    [Fact]
    public void Constructor_InitializesCollections()
    {
        // Assert
        _viewModel.MyRentals.Should().NotBeNull();
        _viewModel.MyRentals.Should().BeEmpty();
    }

    [Fact]
    public void Title_IsSetToRentals()
    {
        // Assert
        _viewModel.Title.Should().Be("Rentals");
    }
}
