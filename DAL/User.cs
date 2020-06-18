using System.Collections.Generic;

namespace DAL
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public virtual List<Video> Videos { get; set; }
    }
}
