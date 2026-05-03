using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

public partial class AppShell : Shell
{
	private bool _isInitialNavigation = true;

	public AppShell(AppShellViewModel viewModel)
	{	
		BindingContext = viewModel;
		InitializeComponent();

		// Register routes for pages that aren't in the main navigation
		Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
		Routing.RegisterRoute("ItemDetailPage", typeof(ItemDetailPage));
		Routing.RegisterRoute("CreateItemPage", typeof(CreateItemPage));
		Routing.RegisterRoute("EditItemPage", typeof(CreateItemPage)); // Same page, different mode

		// Hide flyout on login/register pages
		Navigated += (sender, e) =>
		{
			if (e.Current.Location.OriginalString.Contains("LoginPage") ||
				e.Current.Location.OriginalString.Contains("RegisterPage"))
			{
				FlyoutBehavior = FlyoutBehavior.Disabled;
			}
			else
			{
				FlyoutBehavior = FlyoutBehavior.Flyout;
			}
		};
	}

	protected override async void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);

		// Navigate to login page on first navigation
		if (_isInitialNavigation)
		{
			_isInitialNavigation = false;
			await GoToAsync("//LoginPage");
		}
	}
}
