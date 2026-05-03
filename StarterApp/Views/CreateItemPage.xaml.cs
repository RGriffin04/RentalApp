/// @file CreateItemPage.xaml.cs
/// @brief Code-behind for the create/edit item page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for creating or editing marketplace items
public partial class CreateItemPage : ContentPage
{
    private readonly CreateItemViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public CreateItemPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The create item view model
    public CreateItemPage(CreateItemViewModel viewModel)
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
