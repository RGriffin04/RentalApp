using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RentalApp.Api.Controllers;
using RentalApp.Api.Models;
using RentalApp.Api.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Tests.API;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _controller = new CategoriesController(_mockCategoryRepository.Object);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOkWithCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronic items" },
            new() { Id = 2, Name = "Tools", Description = "Tool items" }
        };
        _mockCategoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<List<CategoryResponse>>().Subject;
        response.Should().HaveCount(2);
        response[0].Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetCategory_WithValidId_ShouldReturnOkWithCategory()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Electronics", Description = "Electronic items" };
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategory(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<CategoryResponse>().Subject;
        response.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act
        var result = await _controller.GetCategory(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
