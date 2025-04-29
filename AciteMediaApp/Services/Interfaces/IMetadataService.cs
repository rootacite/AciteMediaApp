using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AciteMediaApp.Models;

namespace AciteMediaApp.Services.Interfaces
{
    public interface IMetadataService
    {
        public Task<ComicMetadata> ResolveMetadataAsyc(string name);
        public Task<string[]> GetCollectionsAsync();
        public Task UpdateProgressAsync(string name, int progress);

        public Task<string[]> GetVideoClasses();

        public Task<string[]> GetVideos(string vclass);
        public Task<Video> ResloveVideosAsync(string v);
    }
}
