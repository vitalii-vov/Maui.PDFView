# Maui.PDFView
Library for display PDF files in .NET MAUI on Android and iOS (Windows alpha)

https://github.com/vitalii-vov/Maui.PDFView/assets/71486507/4977ede8-c8db-454f-930d-ba2ec704f16d

.NET 8.0
.NET MAUI

| Platform     | Supported |
| :----------- | :-------  |
| Android      | ✅        |
| iOS          | ✅        |
| Windows      | alpha     |

## Installation
```
Install-Package Vitvov.Maui.PDFView
```

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
<ContentPage
    x:Class="Example.Business.UI.Pages.MainPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:pdf="clr-namespace:Maui.PDFView;assembly=Maui.PDFView">

    <!--
    IsHorizontal — Display PDF horizontally
    Uri — Path to the file on the device
    -->
    <pdf:PdfView
        IsHorizontal="{Binding IsHorizontal}"
        Uri="{Binding PdfSource}" />

</ContentPage>
```

```C#
internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private string _pdfSource;

    [RelayCommand] private void ChangeUri()
    {
        //  See the example project to understand how to work with paths.
        PdfSource = "/path/to/file.pdf";
    }
}
```
