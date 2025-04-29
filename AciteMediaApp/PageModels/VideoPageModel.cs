using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AciteMediaApp.PageModels
{
    // https://192.168.1.10/api/video/file?file=Animation/880CEDF-720p.mp4
    public partial class VideoPageModel(HttpClient httpClient, IMetadataService metadataService) : ObservableObject
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IMetadataService _metadataService = metadataService;
        private bool _Appeared = false;

        public string TestUri { get; set; } = @"https://192.168.1.10/api/video/file?file=Animation/880CEDF-720p.mp4";

        public ObservableCollection<string> Collections { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Video> Videos { get; set; } = new ObservableCollection<Video>();

        [ObservableProperty]
        public partial string SelectedCollection { get; set; } = "";
        [ObservableProperty]
        public partial string SearchKeyword { get; set; } = "";

        [RelayCommand]
        public async Task OnAppearing()
        {
            if (_Appeared) { return; }

            var c = await _metadataService.GetVideoClasses();
            Collections.Clear();

            foreach(var i in c)
            {
                Collections.Add(i);
            }

            var t = await _metadataService.GetVideos("");
            foreach (var v in t)
            {
                Videos.Add(await _metadataService.ResloveVideosAsync(v));
            }

            _Appeared = true;
        }

        [RelayCommand]
        public void OnFilter()
        {

        }
    }
}
