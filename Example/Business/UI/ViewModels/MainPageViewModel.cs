using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Business.Services;

namespace Example.Business.UI.ViewModels
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private static double MB = 1048576;
        
        private readonly RepositoryService _repository = new();

        [ObservableProperty] private string _pdfSource;
        [ObservableProperty] private bool _isHorizontal;
        [ObservableProperty] private string _elapsedTime;
        [ObservableProperty] private string _fileSize;

        [RelayCommand] private void Appearing()
        {
            ChangeUri();
        }

        [RelayCommand] private void ChangeUri()
        {
            var t = new Stopwatch();
            t.Start();

            var retVal = _repository.GetPdfSource();
            PdfSource = retVal.Item1;
            t.Stop();
            ElapsedTime = t.Elapsed.ToString();

            FileSize = ((double)retVal.Item2 / MB).ToString("F2") + " MB";

        }
    }
}
