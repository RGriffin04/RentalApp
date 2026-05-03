using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class MyItemsPage : ContentPage
{
    public MyItemsPage(MyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MyItemsViewModel viewModel)
        {
            await viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
