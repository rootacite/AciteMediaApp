using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using AciteMediaApp.Models;

namespace AciteMediaApp.PageModels.ContentModels
{
    public partial class DownloadTaskModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; } = "";
        [ObservableProperty]
        public partial double Progress { get; set; } = 0.0d;
        [ObservableProperty]
        public partial int Total { get; set; } = 0;
        [ObservableProperty]
        public partial int Current { get; set; } = 0;
        [ObservableProperty]
        public partial string StringStatus { get; set; } = "";

        [RelayCommand]
        public async Task OnStop()
        {
            await _cts.CancelAsync();
        }

        public void OnStatusUpdate(Comic comic, int current, int total)
        {
            if (comic.Name != Name)
            {
                return;
            }

            Current = current;
            Total = total;

            StringStatus = $"{Current} / {Total}";
            Progress = (double)Current / (double)Total;
        }

        private readonly CancellationTokenSource _cts;
        private readonly DownloaderService _downloaderService;

        public DownloadTaskModel(Comic content, CancellationTokenSource cts, IComicService comicService, DownloaderService downloaderService)
        {
            Name = content.Name;
            Total = content.TotalPages;
            Progress = 0;
            Current = 0;

            _cts = cts;
            _downloaderService = downloaderService;

            _downloaderService.DownloadUpdated += OnStatusUpdate;
        }
    }
}
