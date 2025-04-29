using System.Collections.ObjectModel;
using AciteMediaApp.Models;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AciteMediaApp.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private IComicService _comicService;

        public ObservableCollection<Comic> Comics { get; set; } = new();

        [ObservableProperty]
        public partial bool? IsRefreshing { get; set; } = false;

        public MainPageModel(IComicService comicService)
        {
            _comicService = comicService;            
            IsRefreshing  = false;
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (IsRefreshing == true) return;

            IsRefreshing = true;
            var comics = await _comicService.GetComicsAsync();
            foreach (string? comic in comics)
            {
                if (comic == null) continue;
                var rc = await _comicService.ResolveAsync(comic);
                if (rc == null) continue;
                Comic cc = rc;

                if (!Comics.Any(x => x.Name == cc.Name))
                    Comics.Add(cc);
            }        

            IsRefreshing = false;
            OnComicUpdated?.Invoke(
                null, 
                new EventArgs());
        }

        public event EventHandler? OnComicUpdated;
    }
}