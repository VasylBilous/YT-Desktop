using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DAL
{
    public class DbWorker
    {
        private Context worker = new Context();

        public void AddUser(User user)
        {
            worker.Users.Add(user);
            worker.SaveChanges();
        }

        public void UpdateUser(User[] users)
        {
            var oldUser = users[0];
            var newUser = users[1];
            var con = worker.Users.Where(x => x.Login == oldUser.Login).FirstOrDefault();

            var oldVideos = (from v in worker.Videos
                             join k in worker.Keys on v.Adress equals k.VideoAdress
                             join u in worker.Users on k.UserEmail equals u.Login
                             select v).ToList();

            if (oldVideos.Count > 0)
            {
                foreach (var video in oldVideos)
                {
                    RemoveVideo(oldUser, video);
                    AddVideo(newUser, video);
                }
                worker.SaveChanges();
            }

            worker.Entry(con).State = EntityState.Deleted;
            worker.Users.Add(newUser);
            worker.SaveChanges();
        }

        public void AddVideo(User user, Video video)
        {
            var findVideo = (from v in worker.Videos
                             join k in worker.Keys on v.Adress equals k.VideoAdress
                             where k.UserEmail == user.Login && v.Adress == video.Adress
                             select v).FirstOrDefault(); 

            var findUser = worker.Users.Where(x => x.Login == user.Login).FirstOrDefault();

            if (findUser != null)
            {
                if (findVideo == null)
                {
                    worker.Videos.Add(video);
                    worker.SaveChanges();

                    Video savedVideo = worker.Videos.Where(x => x.Adress == video.Adress).FirstOrDefault();
                    worker.Keys.Add(new KeyTable { UserEmail = findUser.Login, VideoAdress = savedVideo.Adress });
                    worker.SaveChanges();
                }
            }
        }

        public void RemoveVideo(User user, Video video)
        {
            var findVideo = (from v in worker.Videos
                             join k in worker.Keys on v.Adress equals k.VideoAdress
                             where k.UserEmail == user.Login && v.Adress == video.Adress
                             select v).FirstOrDefault();

            var findKey = (from k in worker.Keys
                           where k.UserEmail == user.Login
                           select k).FirstOrDefault();

            worker.Entry(findKey).State = EntityState.Deleted;
            worker.Entry(findVideo).State = EntityState.Deleted;
            worker.SaveChanges();
        }

        public List<Video> GetAllVideos(User user)
        {
           var r = (from v in worker.Videos
                    join k in worker.Keys on v.Adress equals k.VideoAdress
                    where k.UserEmail == user.Login
                    select v).ToList();
            return r;
        }

        public bool CheckLoginIsAvailable(string login)
        {
            User user = worker.Users.Where(x => x.Login == login).FirstOrDefault();

            if (user == null)
                return true;

            return false;
        }
        public LoginPackage UserLogIn(User user)
        {
            string login = user.Login;
            string password = user.Password;

            User find = worker.Users.Where(x => x.Login == login && x.Password == password).FirstOrDefault();

            if (find == null)
                return new LoginPackage { User = new User(), Res = false };
            else
                return new LoginPackage { User = find, Res = true }; ;
        }

        public void MakeBackup(object o)
        {
            string backupFolder = ConfigurationManager.AppSettings["BackupFolder"];
            string connection = ConfigurationManager.ConnectionStrings["Context"].ConnectionString;

            var sqlConStrBuilder = new SqlConnectionStringBuilder(connection);
            var backupFileName = String.Format("{0}{1}-{2}.bak",
                backupFolder, sqlConStrBuilder.InitialCatalog,
                DateTime.Now.ToString("yyyy-MM-dd"));

            SqlConnection sql = new SqlConnection(connection);
            try
            {
                sql.Open();
                var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                   sqlConStrBuilder.InitialCatalog, backupFileName);
                SqlCommand command = new SqlCommand(query, sql);
                command.ExecuteNonQuery();
            }
            finally
            {
                if (sql != null)
                    sql.Close();
            }
        }
    }

}

