using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using ImageSource = Microsoft.Maui.Controls.ImageSource;

namespace AciteMediaApp.Services
{
    public interface IComicService
    {
        public Task<string[]> GetComicsAsync();
        public Task<Comic?> ResolveAsync(string name);
        public Task<ComicPage> GetPageAsync(Comic comic, int page, bool cacheToDisk, bool update);
    }
}
