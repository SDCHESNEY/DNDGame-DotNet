using DNDGame.MauiApp.ViewModels;

namespace DNDGame.MauiApp.Pages;

public partial class DiceRollerPage : ContentPage
{
    public DiceRollerPage(DiceRollerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
