using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Business.Services;
using Maui.PDFView.Events;
using System.Diagnostics;
using Maui.PDFView.DataSources;

namespace Example.Business.UI.ViewModels
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private readonly IRepositoryService _repository = new RepositoryService();

        [ObservableProperty] private string _pdfSource;
        [ObservableProperty] private bool _isHorizontal;
        [ObservableProperty] private float _maxZoom = 4;
        [ObservableProperty] private string _pagePosition;
        [ObservableProperty] private uint _pageIndex = 0;
        [ObservableProperty] private uint _maxPageIndex = uint.MaxValue;

        [RelayCommand] private void Appearing()
        { 
            ChangeUri();
        }

        [RelayCommand] private void ChangeUri()
        {
            PdfSource = _repository.GetPdfSource();
        }

        [RelayCommand] private void PageChanged(PageChangedEventArgs args)
        {
            MaxPageIndex = (uint)args.TotalPages - 1;
            PagePosition = $"{args.CurrentPage} of {args.TotalPages}";
            Debug.WriteLine($"Current page: {args.CurrentPage} of {args.TotalPages}");
        }
        
        [RelayCommand] private async Task UploadUri()
        {
            // You can use the IPdfSource interface to load a PDF file from various sources such as assets, HTTP, file system, etc.,
            // using the built-in implementations: AssetPdfSource, FilePdfSource, ByteArrayPdfDataSource and HttpPdfSource.
            // Alternatively, you can create your own implementation of IPdfSource to suit your specific needs.
            // Using this interface is optional, as its main purpose is to convert your data format into a file path on the device.
            IPdfSource source;
            //source = new AssetPdfSource("Example.Resources.PDF.pdf2.pdf");
            //source = new FilePdfSource(_repository.GetPdfSource());
            //source = new ByteArrayPdfDataSource(await File.ReadAllBytesAsync(_repository.GetPdfSource()));
            source = new HttpPdfSource("https://www.adobe.com/support/products/enterprise/knowledgecenter/media/c4611_sample_explain.pdf");
            PdfSource = await source.GetFilePathAsync();
        }

    }
}
