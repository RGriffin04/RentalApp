/// @file RentalDetailPage.xaml.cs
/// @brief Code-behind for the rental detail page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for displaying detailed rental information
public partial class RentalDetailPage : ContentPage
{
    private readonly RentalDetailViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public RentalDetailPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The rental detail view model
    public RentalDetailPage(RentalDetailViewModel viewModel)
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
