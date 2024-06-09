using Example.Business.UI.ViewModels;

namespace Example.Business.UI.Pages;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainPageViewModel();
    }
}