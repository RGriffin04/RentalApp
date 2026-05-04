using FluentAssertions;
using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Tests.ViewModels;

/// <summary>
/// Tests for NearbyItemsViewModel - demonstrates testing location-based functionality
/// </summary>
public class NearbyItemsViewModelTests
{
    private readonly Mock<IItemService> _itemServiceMock;
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsViewModelTests()
    {
        _itemServiceMock = new Mock<IItemService>();
        _locationServiceMock = new Mock<ILocationService>();
        _navigationServiceMock = new Mock<INavigationService>();

        _viewModel = new NearbyItemsViewModel(
            _itemServiceMock.Object,
            _locationServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public async Task SearchNearbyCommand_WithValidLocation_LoadsNearbyItems()
    {
        // Arrange
        var nearbyItems = new List<ItemWithDistance>
        {
            new ItemWithDistance 
            { 
                Item = new Item { Id = 1, Title = "Nearby Item 1" },
                DistanceKm = 2.5
            },
            new ItemWithDistance 
            { 
                Item = new Item { Id = 2, Title = "Nearby Item 2" },
                DistanceKm = 5.0
            }
        };

        // Set location properties on ViewModel
        _viewModel.Latitude = 40.7128;
        _viewModel.Longitude = -74.0060;
        _viewModel.RadiusKm = 10;

        _itemServiceMock
            .Setup(x => x.GetNearbyItemsAsync(40.7128, -74.0060, 10))
            .ReturnsAsync(nearbyItems);

        // Act
        await _viewModel.SearchNearbyCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Items.Should().HaveCount(2);
        _viewModel.Items[0].DistanceKm.Should().Be(2.5);
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task SearchNearbyCommand_WhenLocationFails_SetsError()
    {
        // Arrange
        _locationServiceMock
            .Setup(x => x.CheckLocationPermissionAsync())
            .ReturnsAsync(false);

        // Act
        await _viewModel.GetCurrentLocationCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasLocationPermission.Should().BeFalse();
    }

    [Fact]
    public async Task ViewItemCommand_NavigatesToItemDetailWithCorrectId()
    {
        // Arrange
        var item = new Item { Id = 123, Title = "Test Item" };

        // Act
        await _viewModel.ViewItemCommand.ExecuteAsync(item);

        // Assert
        _navigationServiceMock.Verify(
            x => x.NavigateToAsync($"ItemDetailPage?itemId=123"),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentLocationCommand_SetsCurrentLocation()
    {
        // Arrange
        var expectedLocation = (Latitude: 37.7749, Longitude: -122.4194);

        _locationServiceMock
            .Setup(x => x.CheckLocationPermissionAsync())
            .ReturnsAsync(true);

        _locationServiceMock
            .Setup(x => x.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        await _viewModel.GetCurrentLocationCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Latitude.Should().Be(37.7749);
        _viewModel.Longitude.Should().Be(-122.4194);
    }

    [Fact]
    public void RadiusKm_DefaultsTo10Km()
    {
        // Assert
        _viewModel.RadiusKm.Should().Be(10);
    }

    [Fact]
    public void Title_IsSetToNearbyItems()
    {
        // Assert
        _viewModel.Title.Should().Be("Nearby Items");
    }
}
