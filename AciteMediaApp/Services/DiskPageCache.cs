using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;

namespace AciteMediaApp.Services
{
    class DiskPageCache : IPageCache
    {
        public async Task<ComicPage> GetAsync(Comic comic, int page, IPageCache.MissHandler? miss, bool cacheAfterMiss, bool update)
        {
            var dir = Path.Combine(MetadataService.CachePath, comic.Name);
            var file = Path.Combine(dir, comic.Pages[page]);

            if (!Directory.Exists(dir) || !File.Exists(file) || update)
            {
                if (miss == null) throw new IOException("Required Resource is not Found and miss handler is not specified.");

                var c = await miss.Invoke(comic, page);

                if(cacheAfterMiss)
                {
                    if(!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if(update && File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    var fs = File.OpenWrite(file);          
                    var cs = await c.Page.Stream(CancellationToken.None);
                    cs.CopyTo(fs);
                    fs.Close();

                    c.DiskCached = true;
                }

                return c;
            }

            byte[] imgBytes = File.ReadAllBytes(file);
            StreamImageSource? img = ImageSource.FromStream(() => new MemoryStream(imgBytes)) as StreamImageSource;

            if (img == null) throw new IOException("Disk Cache Read Failed");

            return new ComicPage() 
            { 
                Page = img,
                PageNumber = page,
                Life = 0,
                DiskCached = true
            };
        }
    }
}
