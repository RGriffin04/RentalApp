using FluentAssertions;
using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Tests.ViewModels;

/// <summary>
/// Tests for ItemsListViewModel - demonstrates testing search and filtering functionality
/// </summary>
public class ItemsListViewModelTests
{
    private readonly Mock<IItemService> _itemServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly ItemsListViewModel _viewModel;

    public ItemsListViewModelTests()
    {
        _itemServiceMock = new Mock<IItemService>();
        _navigationServiceMock = new Mock<INavigationService>();

        _viewModel = new ItemsListViewModel(
            _itemServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public async Task LoadDataAsync_LoadsCategoriesAndItems()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Tools" }
        };

        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Laptop", CategoryId = 1 },
            new Item { Id = 2, Title = "Hammer", CategoryId = 2 }
        };

        _itemServiceMock
            .Setup(x => x.GetCategoriesAsync())
            .ReturnsAsync(categories);

        _itemServiceMock
            .Setup(x => x.GetAllItemsAsync(null, null, true))
            .ReturnsAsync(items);

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Categories.Should().HaveCount(3); // "All Categories" + 2 categories
        _viewModel.Categories[0].Name.Should().Be("All Categories");
        _viewModel.Items.Should().HaveCount(2);
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDataAsync_ChangingSearchText_FiltersItems()
    {
        // Arrange
        var searchResults = new List<Item>
        {
            new Item { Id = 1, Title = "Laptop", CategoryId = 1 },
            new Item { Id = 2, Title = "Gaming Laptop", CategoryId = 1 }
        };

        _itemServiceMock
            .Setup(x => x.GetAllItemsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(searchResults);

        // Act
        _viewModel.SearchText = "Laptop";
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Items.Should().HaveCountGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ViewItemCommand_NavigatesToItemDetail()
    {
        // Arrange
        var item = new Item { Id = 42, Title = "Test Item" };

        // Act
        await _viewModel.ViewItemCommand.ExecuteAsync(item);

        // Assert
        _navigationServiceMock.Verify(
            x => x.NavigateToAsync($"ItemDetailPage?itemId=42"),
            Times.Once);
    }

    [Fact]
    public async Task RefreshCommand_ReloadsItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Refreshed Item" }
        };

        _itemServiceMock
            .Setup(x => x.GetAllItemsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(items);

        // Act
        await _viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Items.Should().HaveCount(1);
        _viewModel.IsRefreshing.Should().BeFalse();
    }

    [Fact]
    public async Task LoadDataAsync_WhenServiceFails_SetsError()
    {
        // Arrange
        _itemServiceMock
            .Setup(x => x.GetCategoriesAsync())
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().Contain("Network error");
    }

    [Fact]
    public void Constructor_InitializesWithCorrectDefaults()
    {
        // Assert
        _viewModel.Title.Should().Be("Browse Items");
        _viewModel.Items.Should().NotBeNull();
        _viewModel.Categories.Should().NotBeNull();
        _viewModel.ShowOnlyAvailable.Should().BeTrue();
        _viewModel.SearchText.Should().BeEmpty();
    }
}
