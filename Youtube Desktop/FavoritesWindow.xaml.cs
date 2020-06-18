using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Youtube_Desktop.data;

namespace Youtube_Desktop
{
    /// <summary>
    /// Interaction logic for FavoritesWindow.xaml
    /// </summary>
    public partial class FavoritesWindow : Window, INotifyPropertyChanged
    {
        private MainWindow mainWindow;
        private const int Port = 3030;
        private const int Port2 = 3033;
        private NetworkStream stream = null;
        private TcpClient client = null;

        private string search;
        private Video selected;
        private string path;

        public string Path { get => path; set { path = value; NotifyUI(); } }
        public string Search { get => search; set { search = value; NotifyUI(); } }
        public ObservableCollection<Video> Favorites { get; set; } = new ObservableCollection<Video>();
        public Video Selected { get => selected; set { selected = value; Path = Selected.Adress; NotifyUI(); } }
        public User User { get; set; }

        public FavoritesWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            User = mainWindow.User;
            DataContext = this;
            UpdateFavorites();
        }

        private void UpdateFavorites()
        {
            SendCommand("GetAllVideos");

            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(User.GetType());
                    xml.Serialize(stream, User);
                }
            }
            catch (ApplicationException ex)
            {
                mainWindow.LogWork(ex.Message);
            }
            finally { client.Close(); }

            TcpListener serv = null;
            try
            {
                serv = new TcpListener(IPAddress.Parse("127.0.0.1"), Port2);
                serv.Start();
                client = serv.AcceptTcpClient();

                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(typeof(List<Video>));
                    var res = (List<Video>)xml.Deserialize(stream);

                    ObservableCollection<Video> temp = new ObservableCollection<Video>();
                    foreach (var r in res)
                        temp.Add(r);
                    Dispatcher.Invoke(() => Favorites = temp);

                }
            }
            catch (ApplicationException ex) { mainWindow.LogWork(ex.Message); }
            finally { client.Close(); serv.Stop(); }
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            this.Close();
        }

        private void EnterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            mainWindow.Search = Search;
            mainWindow.EnterCommand(null, null);
            mainWindow.Show();
            this.Close();
        }

        private void btnYoutube_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            this.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(mainWindow);
            loginWindow.Show();
            this.Close();
        }

        private void btnFavorites_Click(object sender, RoutedEventArgs e)
        {
            FavoritesWindow videoWindow = new FavoritesWindow(mainWindow);
            videoWindow.Show();
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Search = Search;
            mainWindow.btnSearch_Click(null, null);
            mainWindow.Show();
            this.Close();
        }

        private void VideoField_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mainWindow.Selected = Selected;
            VideoWindow videoWindow = new VideoWindow(mainWindow);
            videoWindow.Show();
            this.Close();
        }

        private void btnRemoveToFavorites_Click(object sender, RoutedEventArgs e) => RemoveVideo();

        private void RemoveVideo()
        {
            SendCommand("RemoveVideo");

            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    PackData pack = new PackData { User = User, Video = Selected };
                    XmlSerializer xml = new XmlSerializer(pack.GetType());
                    xml.Serialize(stream, pack);
                }
            }
            catch (ApplicationException ex)
            {
                mainWindow.LogWork(ex.Message);
            }
            finally { client.Close(); }

            btnFavorites_Click(null, null);
        }

        private void SendCommand(string command)
        {
            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(command.GetType());
                    xml.Serialize(stream, command);
                }
            }
            catch (ApplicationException ex)
            {
                mainWindow.LogWork(ex.Message);
            }
            finally { client.Close(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyUI([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            foreach (var v in stackPanel.Children)
            {
                if (v is Label)
                {
                    if ((v as Label).Visibility == Visibility.Collapsed)
                        Selected = new Video { Adress = (v as Label).Content.ToString() };
                }
            }
        }
    }
}
