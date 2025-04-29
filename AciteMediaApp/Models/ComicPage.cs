using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciteMediaApp.Models
{
    public class ComicPage
    {
        public required StreamImageSource Page { get; set; }
        public required int PageNumber { get; set; }
        public required uint Life { get; set; } // LRU
        public required bool DiskCached { get; set; }
    }
}
