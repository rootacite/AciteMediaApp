namespace AciteMediaApp.Pages;

public partial class DownloaderPage : ContentPage
{
	public DownloaderPage(DownloaderPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
    }
}