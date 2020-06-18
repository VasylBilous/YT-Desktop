using System;

namespace Youtube_Desktop.data
{
    [Serializable]
    public class LoginPackage
    {
        public User User { get; set; }
        public bool Res { get; set; }
    }
}
