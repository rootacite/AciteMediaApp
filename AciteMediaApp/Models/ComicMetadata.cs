using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AciteMediaApp.Models
{
    public class ComicMetadata
    {
        [PrimaryKey]
        public string Name { get; set; } = "";

        public int TotalPages { get; set; } = 0;
        public string Pages { get; set; } = "";  // Splited with \n
        public int Progress { get; set; } = 0;
        public string Marks { get; set; } = ""; // Splited with \n
    }
}
