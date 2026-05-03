/// @file ItemsListPage.xaml.cs
/// @brief Code-behind for the items list page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for displaying and browsing marketplace items
public partial class ItemsListPage : ContentPage
{
    private readonly ItemsListViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public ItemsListPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The items list view model
    public ItemsListPage(ItemsListViewModel viewModel)
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
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }

    /// @brief Event handler for category selection change
    private async void OnCategoryChanged(object sender, EventArgs e)
    {
        if (_viewModel != null && !_viewModel.IsBusy)
        {
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    /// @brief Event handler for available switch toggle
    private async void OnAvailableToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel != null && !_viewModel.IsBusy)
        {
            await _viewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }
}
