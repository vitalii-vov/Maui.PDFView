﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Example.Business.Services;

namespace Example.Business.UI.ViewModels
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private readonly RepositoryService _repository = new();

        [ObservableProperty] private string _pdfSource;
        [ObservableProperty] private bool _isHorizontal;

        [RelayCommand] private void Appearing()
        {
            ChangeUri();
        }

        [RelayCommand] private void ChangeUri()
        {
            PdfSource = _repository.GetPdfSource();
        }
    }
}
