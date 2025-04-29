using System.Text;
using AciteMediaApp.Models;

namespace AciteMediaApp.Pages;

public partial class VideoPage : ContentPage
{
	public VideoPage(VideoPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var video = ((sender as BindableObject)?.BindingContext as Video);

		if (video != null)
		{
            await Shell.Current.GoToAsync($"{"video_viewer"}?name={video.Resolvable}");
        }
    }
}