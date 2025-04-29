using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;

namespace AciteMediaApp.Services.Interfaces
{
    public interface IPageCache
    {
        public delegate Task<ComicPage> MissHandler(Comic comic, int page);
        public Task<ComicPage> GetAsync(Comic comic, int page, MissHandler? miss, bool cacheAfterMiss, bool update);
    }
}
