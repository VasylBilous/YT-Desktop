using System;

namespace Server.data
{
    [Serializable]
    public class Video
    {
        public string Adress { get; set; }
        public string ImageUrl { get; set; }
        public string VideoTitle { get; set; }
        public string ChannelTitle { get; set; }
        public DateTime PostedDate { get; set; }
        public bool IsVideo { get; set; }
        public string Desctiption { get; set; }
    }
}
