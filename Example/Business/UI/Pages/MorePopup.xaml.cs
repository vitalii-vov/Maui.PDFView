using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.PDFView.DataSources;
using Mopups.Services;

namespace Example.Business.UI.Pages;

public partial class MorePopup
{
    public MorePopup()
    {
        InitializeComponent();
        BindingContext = new MorePopupViewModel();
    }
}

public partial class MorePopupViewModel : ObservableObject
{
    [ObservableProperty] private string? _pdfSource;
    
    [RelayCommand] private async Task Appearing()
    { 
        var source = new HttpPdfSource("https://www.adobe.com/support/products/enterprise/knowledgecenter/media/c4611_sample_explain.pdf");
        PdfSource = await source.GetFilePathAsync();
    }
    
    [RelayCommand] private async Task Close()
    { 
        await MopupService.Instance.PopAsync();
    }
}