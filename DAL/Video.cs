using System;
using System.Collections.Generic;

namespace DAL
{
    public class Video
    {
        public int Id { get; set; }
        public string Adress { get; set; }
        public string ImageUrl { get; set; }
        public string VideoTitle { get; set; }
        public string ChannelTitle { get; set; }
        public DateTime PostedDate { get; set; }
        public bool IsVideo { get; set; }
        public string Desctiption { get; set; }
        public virtual List<User> Users { get; set; }
    }
}
