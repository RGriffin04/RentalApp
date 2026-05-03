/// @file RentalsPage.xaml.cs
/// @brief Code-behind for the rentals page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for managing rentals as renter and owner
public partial class RentalsPage : ContentPage
{
    private readonly RentalsViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public RentalsPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The rentals view model
    public RentalsPage(RentalsViewModel viewModel)
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
}
