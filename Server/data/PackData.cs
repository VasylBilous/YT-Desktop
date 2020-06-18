using System;

namespace Server.data
{
    [Serializable]
    public class PackData
    {
        public User User { get; set; }
        public Video Video { get; set; }
    }
}
