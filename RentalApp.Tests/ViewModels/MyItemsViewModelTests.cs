using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Tests.ViewModels;

public class MyItemsViewModelTests
{
    private readonly Mock<IItemService> _mockItemService;
    private readonly Mock<IRentalService> _mockRentalService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly MyItemsViewModel _viewModel;

    public MyItemsViewModelTests()
    {
        _mockItemService = new Mock<IItemService>();
        _mockRentalService = new Mock<IRentalService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new MyItemsViewModel(
            _mockItemService.Object,
            _mockRentalService.Object,
            _mockNavigationService.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithTitle()
    {
        // Assert
        _viewModel.Title.Should().Be("My Items");
        _viewModel.ItemsWithRentals.Should().NotBeNull();
        _viewModel.ItemsWithRentals.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadDataAsync_ShouldLoadItemsWithRentals()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Drill", OwnerId = 1 },
            new Item { Id = 2, Title = "Ladder", OwnerId = 1 }
        };
        var rentals = new List<Rental>
        {
            new Rental { Id = 1, ItemId = 1, Status = "Pending", RenterId = 2 },
            new Rental { Id = 2, ItemId = 1, Status = "Active", RenterId = 3 },
            new Rental { Id = 3, ItemId = 2, Status = "Pending", RenterId = 4 }
        };

        _mockItemService.Setup(s => s.GetMyItemsAsync()).ReturnsAsync(items);
        _mockRentalService.Setup(s => s.GetMyListingsAsync()).ReturnsAsync(rentals);

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ItemsWithRentals.Should().HaveCount(2);
        _viewModel.ItemsWithRentals[0].Item!.Title.Should().Be("Drill");
        _viewModel.ItemsWithRentals[0].Rentals.Should().HaveCount(2);
        _viewModel.ItemsWithRentals[1].Rentals.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadDataAsync_WhenAlreadyBusy_ShouldNotLoadAgain()
    {
        // Arrange
        _viewModel.GetType().GetProperty("IsBusy")!.SetValue(_viewModel, true);

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _mockItemService.Verify(s => s.GetMyItemsAsync(), Times.Never);
    }

    [Fact]
    public async Task LoadDataAsync_WhenServiceFails_ShouldSetError()
    {
        // Arrange
        _mockItemService.Setup(s => s.GetMyItemsAsync()).ThrowsAsync(new Exception("Service error"));

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Contain("Service error");
        _viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public void ItemWithRentals_ShouldCalculatePendingCount()
    {
        // Arrange
        var itemWithRentals = new ItemWithRentals
        {
            Item = new Item { Id = 1, Title = "Drill" }
        };
        itemWithRentals.Rentals.Add(new Rental { Status = "Pending" });
        itemWithRentals.Rentals.Add(new Rental { Status = "Active" });
        itemWithRentals.Rentals.Add(new Rental { Status = "Pending" });

        // Assert
        itemWithRentals.PendingRentalsCount.Should().Be(2);
        itemWithRentals.TotalRentalsCount.Should().Be(3);
    }
}
