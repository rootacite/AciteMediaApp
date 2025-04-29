using System.Xml.Linq;
using AciteMediaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AciteMediaApp.Pages;

public partial class DetailPage : ContentPage
{
    private readonly DetailPageModel _model;
    private async void ErrorHandler(string message)
    {
        // Error
        bool result = await DisplayAlert("Error", $"Info: {message}", "OK", "Cancel");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnStart(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{"comic_reader"}?Name={_model.Name}&Page=-1");
    }

    public DetailPage(DetailPageModel model)
	{
		InitializeComponent();
        BindingContext = model;
        _model = model;

        model.Error += ErrorHandler;

        Disappearing += (e, v) =>
        {
            model.Error -= ErrorHandler;
        };
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var mark = (sender as VisualElement)?.BindingContext as ComicMarks;
        if (mark != null)
        {
            await Shell.Current.GoToAsync($"{"comic_reader"}?Name={_model.Name}&Page={_model.ReserveMap[mark.Page]}");
        }
    }
}