using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AciteMediaApp.PageModels.ContentModels;

namespace AciteMediaApp.PageModels
{
    public partial class DownloaderPageModel : ObservableObject
    {
        private readonly DownloaderService _downloaderService;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<DownloadTaskModel> DownloadTasks { get; private set; } = new ObservableCollection<DownloadTaskModel>();

        public DownloaderPageModel(DownloaderService downloaderService, IServiceProvider serviceProvider)
        {
            _downloaderService = downloaderService;
            _serviceProvider = serviceProvider;
            _downloaderService.DownloadStarted += OnDownloadStarted;
            _downloaderService.DownloadStopped += OnDownloadStopped;
        }

        private void OnDownloadStarted(Comic comic, CancellationTokenSource cts)
        {
            var downloadTask = ActivatorUtilities.CreateInstance<DownloadTaskModel>(
                _serviceProvider,
                comic, cts);

            DownloadTasks.Add(downloadTask);
        }

        private void OnDownloadStopped(Comic comic, int reason)
        {
            if (reason == 0) return;

            for (int i = 0; i < DownloadTasks.Count; i++)
            {
                var downloadTask = DownloadTasks[i];
                if (downloadTask.Name == comic.Name)
                {
                    DownloadTasks.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
