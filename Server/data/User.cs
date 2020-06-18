using System;

namespace Server.data
{
    [Serializable]
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
