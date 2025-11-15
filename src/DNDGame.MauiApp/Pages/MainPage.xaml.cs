using DNDGame.MauiApp.ViewModels;

namespace DNDGame.MauiApp.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
