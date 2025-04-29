using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AciteMediaApp.PageModels
{
    [QueryProperty(nameof(Page), "Page")]
    [QueryProperty(nameof(Name), "Name")]
    public partial class ComicViewerModel(
        IComicService comicService, IMetadataService metadataService,
        DownloaderService downloaderService) : ObservableObject
    {
        private int pageArg = -2;
        public int Page
        {
            get { return pageArg; }
            set
            { 
                pageArg = value;
            }
        }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                _ = OnInitContent(name);
                OnPropertyChanged(nameof(Name));
            }
        }

        private readonly IComicService _comicService = comicService;
        private readonly IMetadataService _metadataService = metadataService;
        private readonly DownloaderService _downloaderService = downloaderService;

        private Comic? _content = null;
        public int PageNumber { get; private set; } = 0;

        [ObservableProperty]
        public partial int TempPage { get; set; }

        public delegate void PageChangedEvent(ComicPage p, int toward);

        public event PageChangedEvent? Switched;
        public event PageChangedEvent? Init;
        public event Action<string>? Error;

        public int? PageCount => _content?.TotalPages;

        private async Task<ComicPage> GetPage(int p)
        {
            if (_content == null) throw new IOException("Must call GetPage after _content is set!");

            var r = await _comicService.GetPageAsync(_content, p, false, false);

            _ = Task.Run(async () =>
            {
                for (int i = p - 2; i <= p + 2 && i < _content.TotalPages; i++)
                {
                    await _comicService.GetPageAsync(_content, int.Max(0, p), false, false); // cache around pages
                }
            });

            return r;
        }

        [RelayCommand]
        public async Task OnInitContent(string name)
        {
            if (name == "") return;
            _content = await _comicService.ResolveAsync(name);

            while (pageArg == -2) await Task.Delay(100);

            if(_content == null)
            {
                Error?.Invoke("ResolveAsync() Error");
                return;
            }

            if (pageArg == -1)
                PageNumber = _content.Progress;
            else
            {
                PageNumber = pageArg;
                _content.Progress = PageNumber;
                await _metadataService.UpdateProgressAsync(_content.Name, _content.Progress);
            }
            TempPage = PageNumber;

            Init?.Invoke(await GetPage(_content.Progress), 0);
            OnPropertyChanged(nameof(PageCount));
        }

        [RelayCommand]
        public async Task OnJump()
        {
            if (_content == null) throw new IOException("Must call after _content is set!");

            var d = TempPage;

            if (d == PageNumber) return;

            Switched?.Invoke(await GetPage(d), d > PageNumber ? 1 : -1);
            PageNumber = d;
            TempPage = PageNumber;
            _content.Progress = PageNumber;
      
            await _metadataService.UpdateProgressAsync(_content.Name, _content.Progress);
        }

        [RelayCommand]
        public async Task OnPrevious()
        {
            if (_content == null) throw new IOException("Must call after _content is set!");

            if (PageNumber > 0) PageNumber -= 1;
            Switched?.Invoke(await GetPage(PageNumber), -1);
            TempPage = PageNumber;
            _content.Progress = PageNumber;

            await _metadataService.UpdateProgressAsync(_content.Name, _content.Progress);
        }

        [RelayCommand]
        public async Task OnNext()
        {
            if (_content == null) throw new IOException("Must call after _content is set!");

            if (PageNumber < _content.TotalPages - 1) PageNumber += 1;
            Switched?.Invoke(await GetPage(PageNumber), 1);
            TempPage = PageNumber;
            _content.Progress = PageNumber;

            await _metadataService.UpdateProgressAsync(_content.Name, _content.Progress);
        }

        [RelayCommand]
        public void OnDownload()
        {
            if (_content == null) throw new IOException("Must call after _content is set!");
            CancellationTokenSource cts = new CancellationTokenSource();
            _ = _downloaderService.Start(
                _content,
                cts
            );
        }
    }
}
