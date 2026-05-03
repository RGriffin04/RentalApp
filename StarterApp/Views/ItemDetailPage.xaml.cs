/// @file ItemDetailPage.xaml.cs
/// @brief Code-behind for the item detail page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for displaying detailed item information
public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public ItemDetailPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The item detail view model
    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// @brief Called when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            await _viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
