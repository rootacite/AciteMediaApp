using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciteMediaApp.Models
{
    public class ComicMarks
    {
        public required string Name { get; set; }
        public required string Page { get; set; }
        public required StreamImageSource? PageContent { get; set; }
    }
}
