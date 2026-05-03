/// @file NearbyItemsPage.xaml.cs
/// @brief Code-behind for the nearby items page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for location-based item search
public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public NearbyItemsPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The nearby items view model
    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// @brief Called when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Optionally auto-load current location on first appearance
        if (_viewModel != null && _viewModel.Latitude == 0 && _viewModel.Longitude == 0)
        {
            // User can manually trigger location
        }
    }
}
