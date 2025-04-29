using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciteMediaApp.Models
{
    public class Video
    {
        public required string Name { get; set; }
        public required ImageSource Cover { get; set; }
        public required string Uri { get; set; }
        public required string Resolvable { get; set; }
    }
}
