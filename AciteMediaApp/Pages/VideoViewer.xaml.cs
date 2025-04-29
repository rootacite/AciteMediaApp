

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
}