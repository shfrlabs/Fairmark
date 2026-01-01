using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairmark.Models
{
    public class Announcement
    {
        public int id { get; set; }
        public string message { get; set; }
        public string from { get; set; }
        public string until { get; set; }
    }
}
