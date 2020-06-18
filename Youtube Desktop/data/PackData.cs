using System;

namespace Youtube_Desktop.data
{
    [Serializable]
    public class PackData
    {
        public User User { get; set; }
        public Video Video { get; set; }
    }
}
