using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AciteMediaApp.Models
{
    public class Comic
    {
        public required string Name { get; set; }
        public ImageSource? CoverImage { get; set; }
        public required int TotalPages { get; set; }
        public required string[] Pages { get; set; }
        public required int Progress { get; set; }
        public required KeyValuePair<string, string>[] Marks { get; set; }
    }
}
