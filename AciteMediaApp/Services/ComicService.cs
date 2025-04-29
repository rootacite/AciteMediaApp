using System;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;
using ImageSource = Microsoft.Maui.Controls.ImageSource;

namespace AciteMediaApp.Services
{
    public class AlphanumComparator : IComparer<string>
    {
        private enum ChunkType
        {
            Alphanumeric,
            Numeric
        };

        private static bool InChunk(char ch, char otherCh)
        {
            var type = ChunkType.Alphanumeric;

            if (char.IsDigit(otherCh))
            {
                type = ChunkType.Numeric;
            }

            return (type != ChunkType.Alphanumeric || !char.IsDigit(ch))
                   && (type != ChunkType.Numeric || char.IsDigit(ch));
        }

        public int Compare(string? x, string? y)
        {
            if (x is not string s1 || y is not string s2)
            {
                return 0;
            }

            int thisMarker = 0;
            int thatMarker = 0;

            while ((thisMarker < s1.Length) || (thatMarker < s2.Length))
            {
                if (thisMarker >= s1.Length)
                {
                    return -1;
                }
                else if (thatMarker >= s2.Length)
                {
                    return 1;
                }

                var thisCh = s1[thisMarker];
                var thatCh = s2[thatMarker];

                var thisChunk = new StringBuilder();
                var thatChunk = new StringBuilder();

                while ((thisMarker < s1.Length) && (thisChunk.Length == 0 || InChunk(thisCh, thisChunk[0])))
                {
                    thisChunk.Append(thisCh);
                    thisMarker++;

                    if (thisMarker < s1.Length)
                    {
                        thisCh = s1[thisMarker];
                    }
                }

                while ((thatMarker < s2.Length) && (thatChunk.Length == 0 || InChunk(thatCh, thatChunk[0])))
                {
                    thatChunk.Append(thatCh);
                    thatMarker++;

                    if (thatMarker < s2.Length)
                    {
                        thatCh = s2[thatMarker];
                    }
                }

                var result = 0;
                // If both chunks contain numeric characters, sort them numerically
                if (char.IsDigit(thisChunk[0]) && char.IsDigit(thatChunk[0]))
                {
                    var thisNumericChunk = Convert.ToInt32(thisChunk.ToString());
                    var thatNumericChunk = Convert.ToInt32(thatChunk.ToString());

                    if (thisNumericChunk < thatNumericChunk)
                    {
                        result = -1;
                    }

                    if (thisNumericChunk > thatNumericChunk)
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = String.Compare(thisChunk.ToString(), thatChunk.ToString(), StringComparison.Ordinal);
                }

                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }
    }


    public class ComicService(
        IMetadataService metadata, 
        [FromKeyedServices("mc")] IPageCache memoryCache,
        [FromKeyedServices("dc")] IPageCache diskCache,
        [FromKeyedServices("wc")] IPageCache webCache) : IComicService
    {

        private readonly AlphanumComparator alphanumComparator = new();

        private readonly IMetadataService _metadata = metadata;
        private readonly IPageCache _memoryCache = memoryCache;
        private readonly IPageCache _diskCache = diskCache;
        private readonly IPageCache _webCache = webCache;

        private KeyValuePair<string, string>[] StringPraseToKeys(string src)
        {
            return src.Split('\n').Select(x => new KeyValuePair<string, string>(x.Split(':')[0], x.Split(':')[1])).ToArray();
        }

        public async Task<Comic?> ResolveAsync(string name)
        {
            try
            {
                ComicMetadata metadata = await _metadata.ResolveMetadataAsyc(name);

                Comic comic = new Comic()
                {
                    Name = name,
                    Pages = metadata.Pages.Split('\n'),
                    TotalPages = metadata.TotalPages,
                    Progress = metadata.Progress,
                    CoverImage = null,
                    Marks = StringPraseToKeys(metadata.Marks)
                };

                comic.CoverImage = (await GetPageAsync(comic, 0, true, false)).Page;

                return comic;
            }
            catch (Exception) { return null; }
        }

        public async Task<string[]> GetComicsAsync()
        {
           return await _metadata.GetCollectionsAsync();
        }

        public async Task<ComicPage> GetPageAsync(Comic comic, int page, bool cacheToDisk, bool update)
        {
            try
            {
                var r = await _memoryCache.GetAsync(comic, page, async (c, p) =>
                                await _diskCache.GetAsync(c, p, async (c1, p1) =>
                                    await _webCache.GetAsync(c1, p1, null, false, false)
                                , cacheToDisk, update)
                                , true, update);
                return r;
            }
            catch (Exception) 
            {
                // using var stream = await FileSystem.OpenAppPackageFileAsync("image.png");
                // ImageSource imageSource = ImageSource.FromStream(() => stream);
                if(ErrorImage == null)
                {
                    throw new Exception("Unknown Error.");
                }

                var ep = new ComicPage()
                {
                    Page = ErrorImage,
                    PageNumber = page,
                    Life = 0,
                    DiskCached = false,
                };
                return ep;
            }
        }

        static readonly public StreamImageSource? ErrorImage = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(HardCodedImages.ExceptionImage))) as StreamImageSource;
    }
}
