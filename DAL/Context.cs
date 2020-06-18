namespace DAL
{
    using System.Data.Entity;

    public class Context : DbContext
    {
        public Context()
            : base("name=Context")
        {
        }
        public virtual DbSet<Video> Videos { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<KeyTable> Keys { get; set; }
    }
}