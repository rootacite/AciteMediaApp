using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;


namespace AciteMediaApp.PageModels
{

    [QueryProperty(nameof(Name), "name")]
    public partial class VideoViewerModel(IMetadataService metadataService): ObservableObject
    {
        private readonly IMetadataService _metadataService = metadataService;

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                _ = InitAsync();
                OnPropertyChanged(nameof(Name));
            }
        }

        [ObservableProperty]
        public partial string VideoUri { get; set; } = "";

        private async Task InitAsync()
        {
            var v = await _metadataService.ResloveVideosAsync(Name);
            VideoUri = v.Uri;
        }
    }
}
