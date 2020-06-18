using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using ClientUser = Server.data.User;
using DbUser = DAL.User;
using ClientVideo = Server.data.Video;
using DbVideo = DAL.Video;
using ClientPackData = Server.data.PackData;
using ClientPackage = Server.data.LoginPackage;
using DbPackage = DAL.LoginPackage;

namespace Server
{
    class Program
    {
        private static DbWorker worker = new DbWorker();
        private static readonly int port = 3030;
        private static readonly int port2 = 3033;
        private static string operation = "";

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int Handle, int showState);
        [DllImport("kernel32.dll")]
        public static extern int GetConsoleWindow();

        static void Main(string[] args)
        {
            // hide app
            //int win = GetConsoleWindow();
            //ShowWindow(win, 0);

            //backup 24h
            TimerCallback tm = new TimerCallback(worker.MakeBackup);
            Timer timer = new Timer(tm, null, 86400000, 86400000);

        restart:
            TcpListener server = null;
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                server.Start();
                while (true)
                {
                    LogWork("Waiting for connection");
                    TcpClient client = null;
                    try
                    {
                        client = server.AcceptTcpClient();
                        LogWork($"Connected to {client.Client.RemoteEndPoint}");
                        using (NetworkStream stream = client.GetStream())
                        {
                            XmlSerializer xml = new XmlSerializer(typeof(string));
                            operation = (string)xml.Deserialize(stream);
                        }
                        LogWork("Waiting for data");
                        try
                        {
                            client = server.AcceptTcpClient();
                            LogWork($"Connected to {client.Client.RemoteEndPoint}");
                            using (NetworkStream stream = client.GetStream())
                            {
                                DoWork(stream);
                            }
                        }
                        catch (SocketException e) { LogWork(e.Message); }
                    }
                    catch (SocketException e) { LogWork(e.Message); }
                    finally { client.Close(); }
                }
            }
            catch (Exception e) { LogWork(e.Message); goto restart; }
        }

        private static void DoWork(NetworkStream stream)
        {
            switch (operation)
            {
                case "SaveUser":
                    SaveUser(stream);
                    break;
                case "LoginCheck":
                    LoginCheck(stream);
                    break;
                case "UserLogin":
                    UserLogin(stream);
                    break;
                case "UpdateUser":
                    UpdateUser(stream);
                    break;
                case "AddVideo":
                    AddVideo(stream);
                    break;
                case "GetAllVideos":
                    GetAllVideos(stream);
                    break;
                case "RemoveVideo":
                    RemoveVideo(stream);
                    break;
            }
        }

        private static void RemoveVideo(NetworkStream stream)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientPackData));
            var data = (ClientPackData)xml.Deserialize(stream);

            if (data.User != null && data.Video != null)
            {
                ClientUser user = data.User;
                ClientVideo video = data.Video;

                worker.RemoveVideo(new DbUser
                {
                    Login = user.Login,
                    Password = user.Password,
                    Token = user.Token
                }, new DbVideo
                {
                    Adress = video.Adress,
                    ChannelTitle = video.ChannelTitle,
                    Desctiption = video.Desctiption,
                    ImageUrl = video.ImageUrl,
                    IsVideo = video.IsVideo,
                    PostedDate = video.PostedDate,
                    VideoTitle = video.VideoTitle
                });
            }
        }

        private static void GetAllVideos(NetworkStream stream)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientUser));
            ClientUser user = (ClientUser)xml.Deserialize(stream);
            var videos = worker.GetAllVideos(new DbUser { Login = user.Login });

            List<ClientVideo> videoPackage = new List<ClientVideo>();
            foreach (var v in videos)
                videoPackage.Add(new ClientVideo
                {
                    Adress = v.Adress,
                    ChannelTitle = v.ChannelTitle,
                    Desctiption = v.Desctiption,
                    ImageUrl = v.ImageUrl,
                    IsVideo = v.IsVideo,
                    PostedDate = v.PostedDate,
                    VideoTitle = v.VideoTitle
                });

            TcpClient tcp = new TcpClient();
            try
            {
                tcp.Connect(IPAddress.Parse("127.0.0.1"), port2);
                using (stream = tcp.GetStream())
                {
                    XmlSerializer xmlll = new XmlSerializer(videoPackage.GetType());
                    xmlll.Serialize(stream, videoPackage);
                }
            }
            catch (Exception e)
            { LogWork(e.Message); }
            finally
            { tcp.Close(); }

        }

        private static void AddVideo(NetworkStream stream)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientPackData));
            var data = (ClientPackData)xml.Deserialize(stream);
            ClientUser user = data.User;
            ClientVideo video = data.Video;

            worker.AddVideo(new DbUser
            {
                Login = user.Login,
                Password = user.Password,
                Token = user.Token
            }, new DbVideo
            {
                Adress = video.Adress,
                ChannelTitle = video.ChannelTitle,
                Desctiption = video.Desctiption,
                ImageUrl = video.ImageUrl,
                IsVideo = video.IsVideo,
                PostedDate = video.PostedDate,
                VideoTitle = video.VideoTitle
            });
        }

        private static void UpdateUser(NetworkStream stream)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientUser[]));
            ClientUser[] users = (ClientUser[])xml.Deserialize(stream);
            worker.UpdateUser(new DbUser[] { new DbUser
            {
                Login = users[0].Login,
                Password = users[0].Password,
                Token = users[0].Token
            }, new DbUser
            {
                 Login = users[1].Login,
                Password = users[1].Password,
                Token = users[1].Token
            } });
        }

        private static void SaveUser(NetworkStream stream)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ClientUser));
            ClientUser user = (ClientUser)xml.Deserialize(stream);
            worker.AddUser(new DbUser { Login = user.Login, Password = user.Password });
        }

        private static void LoginCheck(NetworkStream stream)
        {
            XmlSerializer xmll = new XmlSerializer(typeof(string));
            string login = (string)xmll.Deserialize(stream);
            bool res = worker.CheckLoginIsAvailable(login);
            TcpClient tcp = new TcpClient();
            try
            {
                tcp.Connect(IPAddress.Parse("127.0.0.1"), port2);
                using (stream = tcp.GetStream())
                {
                    XmlSerializer xmlll = new XmlSerializer(res.GetType());
                    xmlll.Serialize(stream, res);
                }
            }
            catch (Exception e)
            { LogWork(e.Message); }
            finally
            { tcp.Close(); }
        }

        private static void UserLogin(NetworkStream stream)
        {
            XmlSerializer xm = new XmlSerializer(typeof(ClientUser));
            ClientUser package = (ClientUser)xm.Deserialize(stream);
            DbPackage dbPackage = worker.UserLogIn(new DbUser
            {
                Login = package.Login,
                Password = package.Password,
                Token = package.Token
            });

            TcpClient tcpp = new TcpClient();
            try
            {
                tcpp.Connect(IPAddress.Parse("127.0.0.1"), port2);
                using (stream = tcpp.GetStream())
                {
                    ClientPackage pack = new ClientPackage();
                    if (dbPackage.Res)
                    {

                        pack.User = new ClientUser
                        {
                            Login = dbPackage.User.Login,
                            Password = dbPackage.User.Password,
                            Token = dbPackage.User.Token
                        };
                    }
                    else
                        pack.User = new ClientUser();

                    pack.Res = dbPackage.Res;

                    XmlSerializer xmlll = new XmlSerializer(pack.GetType());
                    xmlll.Serialize(stream, pack);
                }
            }
            catch (Exception e)
            { LogWork(e.Message); }
            finally
            { tcpp.Close(); }
        }

        static private void LogWork(string text)
        {
            string backupFolder = ConfigurationManager.AppSettings["BackupFolder"];
            using (StreamWriter writer = new StreamWriter(backupFolder + "Youtube Server Log.txt", true))
            {
                writer.WriteLine(DateTime.Now + "\t" + text);
            }
        }
    }
}
