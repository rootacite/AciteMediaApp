using AciteMediaApp.Models;
using AciteMediaApp.PageModels;

namespace AciteMediaApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model, DownloaderPageModel modelDownloader)
        {
            InitializeComponent();
            BindingContext = model;

            // Require modelDownloader to force DownloaderPageModel to Load

            model.OnComicUpdated += (e, v) =>
            {
                comicsLayer.InvalidateMeasure();
            };
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            var comic = ((sender as BindableObject)?.BindingContext as Comic);
            if (comic != null)
            {
                // await Shell.Current.GoToAsync($"{"comic_reader"}?Name={comic.Name}");
                await Shell.Current.GoToAsync($"{"detail_page"}?Name={comic.Name}");
            }
        }
    }
}