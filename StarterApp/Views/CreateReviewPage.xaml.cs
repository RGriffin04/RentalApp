/// @file CreateReviewPage.xaml.cs
/// @brief Code-behind for the create review page
/// @author RentalApp Development Team
/// @date 2025

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// @brief Page for creating reviews for completed rentals
public partial class CreateReviewPage : ContentPage
{
    private readonly CreateReviewViewModel _viewModel;

    /// @brief Default constructor for XAML designer
    public CreateReviewPage()
    {
        InitializeComponent();
    }

    /// @brief Constructor with dependency injection
    /// @param viewModel The create review view model
    public CreateReviewPage(CreateReviewViewModel viewModel)
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

            // Set initial picker selection based on Stars value (default is 5)
            if (this.FindByName("RatingPicker") is Picker picker)
            {
                picker.SelectedIndex = _viewModel.Stars - 1;
            }
        }
    }

    /// @brief Handle rating picker change
    private void OnRatingChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && _viewModel != null)
        {
            _viewModel.Stars = picker.SelectedIndex + 1;
        }
    }
}
