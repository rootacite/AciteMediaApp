using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;

namespace AciteMediaApp.Services
{
    public class DownloaderService(IComicService comicService)
    {
        private readonly IComicService _comicService = comicService;

        public event Action<Comic, CancellationTokenSource>? DownloadStarted;
        // 1. name, 2. reason(0: complete, 1: canceled, 2: error)
        public event Action<Comic, int>? DownloadStopped;
        public event Action<Comic, int, int>? DownloadUpdated;

        private List<string> _downloading = new List<string>();

        public async Task Start(Comic comic, CancellationTokenSource cts)
        {
            if(_downloading.Contains(comic.Name)) { return; }

            var tk = cts.Token;

            DownloadStarted?.Invoke(comic, cts);
            _downloading.Add(comic.Name);

            for (int i = 0; i < comic.TotalPages; i++)
            {
                if(tk.IsCancellationRequested)
                {
                    DownloadStopped?.Invoke(comic, 1);
                }

                await _comicService.GetPageAsync(comic, i, true, true);

                DownloadUpdated?.Invoke(comic, i + 1, comic.TotalPages);
            }

            _downloading.Remove(comic.Name);
            DownloadStopped?.Invoke(comic, 0);
        }
    }
}
