
using System.Collections;
using AciteMediaApp.Models;

namespace AciteMediaApp.Pages;

public partial class ComicViewer : ContentPage
{
    private ComicViewerModel ViewModel { get; set; }

    private static void SetBindingContextRecursively(VisualElement parent, object bindingContext)
    {
        if (parent == null)
            return;

        parent.BindingContext = bindingContext;

        switch (parent)
        {
            case Layout layout:
                foreach (var child in layout.Children)
                {
                    if (child is VisualElement visualChild)
                    {
                        SetBindingContextRecursively(visualChild, bindingContext);
                    }
                }
                break;

            case ContentView contentView:
                if (contentView.Content is VisualElement contentChild)
                {
                    SetBindingContextRecursively(contentChild, bindingContext);
                }
                break;
        }
    }

    private TemplatedView SummonPage(ComicPage p, double offset)
    {
        var grid = new TemplatedView();
        grid.TranslationX = offset;
        grid.BindingContext = p;
        grid.ControlTemplate = (ControlTemplate)Resources["FrameTemplate"];
        SetBindingContextRecursively(grid.Children[0] as VisualElement, p);
        ImageCollection.Children.Add(grid);

        return grid;
    }

    private async void ErrorHandler(string message)
    {
        // Error
        bool result = await DisplayAlert("Error", $"Info: {message}", "OK", "Cancel");
        await Shell.Current.GoToAsync("..");
    }

    private void InitHandler(ComicPage page, int toward)
    {
        SummonPage(page, 0);
    }

    private void SwitchedHandler(ComicPage page, int toward)
    {
        var c = ImageCollection.Children[0] as TemplatedView;
        if (c == null) return;

        if (toward > 0)
        {
            var n = SummonPage(page, c.Width);

            var anime = new Animation(v =>
            {
                c.TranslationX = -v;
                n.TranslationX = c.Width - v;
            }, 0, c.Width);

            anime.Commit(c, "TranslateAnimation", 2, 100, Easing.Linear, (x, ck) =>
            {
                ImageCollection.RemoveAt(0);
            });
        }
        else if (toward < 0)
        {
            var n = SummonPage(page, -c.Width);

            var anime = new Animation(v =>
            {
                c.TranslationX = v;
                n.TranslationX = -c.Width + v;
            }, 0, c.Width);

            anime.Commit(c, "TranslateAnimation", 2, 100, Easing.Linear, (x, ck) =>
            {
                ImageCollection.RemoveAt(0);
            });
        }
    }

    public ComicViewer(ComicViewerModel model)
	{
		InitializeComponent();
		BindingContext = model;
        ViewModel = model;

        model.Error += ErrorHandler;
        model.Init += InitHandler;
        model.Switched += SwitchedHandler;

        Disappearing += (e, v) =>
        {
            model.Error -= ErrorHandler;
            model.Init -= InitHandler;
            model.Switched -= SwitchedHandler;
        };
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}