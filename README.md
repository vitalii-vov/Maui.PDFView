# Maui.PDFView
Library for display PDF files in .NET MAUI on Android, iOS, MacOS and Windows

![NuGet Downloads](https://img.shields.io/nuget/dt/Vitvov.Maui.PDFView?style=for-the-badge)
![last commit](https://img.shields.io/github/last-commit/vitalii-vov/Maui.PDFView?style=for-the-badge)

| .NET 8.0 | .NET MAUI |
| :------- | :-------  |

| Platform  | Android | iOS | MacOS | Windows |
| :-------- | :-----  | :-- | :---- | :------ |
| Supported | ✅      | ✅   | ✅    | ✅      |

https://github.com/vitalii-vov/Maui.PDFView/assets/71486507/4977ede8-c8db-454f-930d-ba2ec704f16d



## Installation
```
Install-Package Vitvov.Maui.PDFView
```

## Usage

> [!IMPORTANT]
> To use a component with `.net9` add `HandlerProperties.DisconnectPolicy="Manual"` to `PdfView`
> ```XAML
> <pdf:PdfView
>     HandlerProperties.DisconnectPolicy="Manual" />
> ```


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
    IsHorizontal — display PDF horizontally
    Uri — path to the file on the device
    MaxZoom — max zoom level
    PageChangedCommand — event of changing the current page
    -->
    <pdf:PdfView
        IsHorizontal="{Binding IsHorizontal}"
        Uri="{Binding PdfSource}"
        MaxZoom="4"
        PageChangedCommand="{Binding PageChangedCommand}">
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

## Personalization
You can customize the way pages are displayed by modifying the `PageAppearance` property in the `PdfView` component.
```xaml
<pdf:PdfView Uri="{Binding PdfSource}">

    <!--
    Margin — Adds indents around pages
    ShadowEnabled — Adds a shadow to each page
    Crop — crops out the part around the page
    -->
    <pdf:PdfView.PageAppearance>
        <pdf:PageAppearance 
            Margin="16,8" 
            ShadowEnabled="True"
            Crop="64,32"/>
    </pdf:PdfView.PageAppearance>

</pdf:PdfView>
```
