## Usage
```C#
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiPdfView()   // <- Write this
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        return builder.Build();
    }
}
```

```xaml
<!--
IsHorizontal — Display PDF horizontally
Uri — Path to the file on the device
-->
<pdf:PdfView
    IsHorizontal="{Binding IsHorizontal}"
    Uri="{Binding PdfSource}" />
```

```C#
internal partial class MainPageViewModel : ObservableObject
{
    private readonly RepositoryService _repository = new();

    [ObservableProperty] private string _pdfSource;
    [ObservableProperty] private bool _isHorizontal;

    [RelayCommand] private void ChangeUri()
    {
        PdfSource = "/path/to/file.pdf";
    }
}
```