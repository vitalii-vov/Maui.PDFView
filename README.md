# Maui.PDFView
Library for display PDF files in .NET MAUI on Android, iOS and Windows

![NuGet Downloads](https://img.shields.io/nuget/dt/Vitvov.Maui.PDFView?style=for-the-badge)

https://github.com/vitalii-vov/Maui.PDFView/assets/71486507/4977ede8-c8db-454f-930d-ba2ec704f16d

.NET 8.0
.NET MAUI

| Platform     | Supported |
| :----------- | :-------  |
| Android      | ✅        |
| iOS          | ✅        |
| MacOS        | ✅        |
| Windows      | ✅        |

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
    IsHorizontal        — Display PDF horizontally
    Uri                 — Path to the file on the device
    MaxZoom             — Max zoom level
    PageChangedCommand  — event of changing the current page
    -->
    <pdf:PdfView
        IsHorizontal="{Binding IsHorizontal}"
        Uri="{Binding PdfSource}"
        MaxZoom="4"
        PageChangedCommand="{Binding PageChangedCommand}">

        <!--
            Margin          — Page Margin
            ShadowEnabled   — Page Shadow
        -->
        <pdf:PdfView.PageAppearance>
            <pdf:PageAppearance Margin="16,8" ShadowEnabled="True" />
        </pdf:PdfView.PageAppearance>

    </pdf:PdfView>

</ContentPage>
```

```C#
internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private string _pdfSource;

    [RelayCommand] private void ChangeUri()
    {
        try 
        {
            //  See the example project to understand how to work with paths.
            PdfSource = "/path/to/file.pdf";
        }
        catch(Exception ex)
        {
             // handle exceptions
        }
    }
}
```
