using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AciteMediaApp.PageModels
{
    [QueryProperty(nameof(Name), "Name")]
    public partial class DetailPageModel(IComicService comicService): ObservableObject
    {
        private IComicService _comicService = comicService;
        public readonly Dictionary<string, int> ReserveMap = new Dictionary<string, int>();

        [ObservableProperty]
        public partial Comic? Content { get; set; } = null;
        public ObservableCollection<ComicMarks> Marks { get; set; } = new ObservableCollection<ComicMarks>();

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                _ = OnInitContent(name);
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool isLoaded = false;

        public async Task OnInitContent(string name)
        {
            if (name == "") return;
            if (isLoaded) { return; }
            Content = await _comicService.ResolveAsync(name);

            if (Content == null)
            {
                Error?.Invoke("ResolveAsync() Error");
                return;
            }

            for (int i = 0; i < Content.TotalPages; i++)
            {
                ReserveMap.Add(Content.Pages[i], i);
            }

            foreach (var mark in Content.Marks)
            {
                var markContent = await _comicService.GetPageAsync(Content, ReserveMap[mark.Value], true, false);
                Marks.Add(new ComicMarks() { Name = mark.Key, Page = mark.Value, PageContent = markContent.Page });
            }

            isLoaded = true;
        }

        public event Action<string>? Error;
    }
}
