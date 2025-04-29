using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;

namespace AciteMediaApp.Services
{
    class MemoryPageCache : IPageCache
    {
        private static string BuildKey(Comic comic, int page) => $"{comic.Name}/{page.ToString("000")}";

        private readonly static Dictionary<string, ComicPage> CachedPages = new Dictionary<string, ComicPage>();

        public async Task<ComicPage> GetAsync(Comic comic, int page, IPageCache.MissHandler? miss, bool cacheAfterMiss, bool update)
        {
            var k = BuildKey(comic, page);

            if (CachedPages.ContainsKey(k) && (!update))
            {
                foreach (var item in CachedPages.Keys.ToArray())
                {
                    if (CachedPages[item].Life < CachedPages[k].Life)
                        CachedPages[item].Life += 1;
                }

                CachedPages[k].Life = 0;
                return CachedPages[k];
            }

            if (miss == null) throw new IOException("Required Resource is not Found and miss handler is not specified.");

            var c = await miss.Invoke(comic, page);

            if (cacheAfterMiss)
            {
                if(update && CachedPages.ContainsKey(k))
                {
                    CachedPages.Remove(k);
                }

                foreach (var item in CachedPages.Keys.ToArray())
                {
                    CachedPages[item].Life += 1;
                }

                CachedPages.Add(k, c);

                if (CachedPages.Count > 1024)
                {
                    var h = CachedPages.MaxBy(x => x.Value.Life).Key;
                    CachedPages.Remove(h);
                }
            }

            return c;
        }
    }
}
