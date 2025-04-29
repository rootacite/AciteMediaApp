using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;

namespace AciteMediaApp.Services
{
    class WebPageCache(HttpClient httpClient) : IPageCache
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ComicPage> GetAsync(Comic comic, int page, IPageCache.MissHandler? miss, bool cacheAfterMiss, bool update)
        {
            if (MetadataService.IsOffLine) throw new IOException("WebCache is unavalible in Offline Mode!");

            var url = MetadataService.BuildUrlWithParameters("api/image/file", new Dictionary<string, string>
            {
                ["file"] = comic.Pages[page],
                ["collection"] = comic.Name
            });

            var imageBytes = await _httpClient.GetByteArrayAsync(url);

            if (imageBytes == null) throw new Exception("Comic Page is not Found in Any Cache!");
            StreamImageSource? img = ImageSource.FromStream(() => new MemoryStream(imageBytes)) as StreamImageSource;
            if (img == null) throw new Exception("Comic Page is not Found in Any Cache!");

            return new ComicPage()
            {
                Page = img,
                PageNumber = page,
                Life = 0,
                DiskCached = false,
            };
        }
    }
}
