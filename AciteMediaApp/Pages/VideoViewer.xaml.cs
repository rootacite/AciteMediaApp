

using Microsoft.Extensions.Logging;

namespace AciteMediaApp.Pages;


[QueryProperty(nameof(Name), "name")]
public partial class VideoViewer : ContentPage
{
    private string name = "";
    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public VideoViewer(VideoViewerModel model)
	{
		InitializeComponent();

        BindingContext = model;
	}

    private void videoViewer_Appearing(object sender, EventArgs e)
    {
        
    }
     
    private void Frame_LongPressed(object sender, MR.Gestures.LongPressEventArgs e)
    {
        MediaPlant.Speed = 1.0;
    }

    private void Frame_LongPressing(object sender, MR.Gestures.LongPressEventArgs e)
    {
        MediaPlant.Speed = 3.0;
    }
}