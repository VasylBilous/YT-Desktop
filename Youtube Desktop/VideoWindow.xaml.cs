using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using VideoLibrary;
using Youtube_Desktop.data;
using NReco.VideoConverter;

namespace Youtube_Desktop
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window, INotifyPropertyChanged
    {
        private MainWindow mainWindow;
        private double oldBrowserHeight = 0;
        private double oldBrowserWidth = 0;
        private const int Port = 3030;
        private NetworkStream stream = null;
        private TcpClient client = null;
        private User user;
        private string info;

        public string Info { get => info; set { info = value; NotifyUI(); } }
        public string Search { get; set; }
        public Video Video { get; set; }
        public User User { get => user; set { user = value; if (user != null) btnFavorites.Visibility = Visibility.Visible; } }

        public VideoWindow(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
            Video = main.Selected;
            User = main.User;
            DataContext = this;
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            this.Close();
        }

        private void FullScreenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            oldBrowserHeight = Browser.Height;
            oldBrowserWidth = Browser.Width;

            topPanel.Visibility = Visibility.Collapsed;
            botPanel.Visibility = Visibility.Collapsed;
            WindowState = WindowState.Maximized;

            Browser.Height += Height - oldBrowserHeight;
            Browser.Width += Width - oldBrowserWidth;
        }

        private void LeaveFullScreenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Browser.Height = oldBrowserHeight;
            Browser.Width = oldBrowserWidth;

            topPanel.Visibility = Visibility.Visible;
            botPanel.Visibility = Visibility.Visible;
            WindowState = WindowState.Normal;
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
            if (User != null)
            {
                FavoritesWindow videoWindow = new FavoritesWindow(mainWindow);
                videoWindow.Show();
                this.Close();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Search = Search;
            mainWindow.btnSearch_Click(null, null);
            mainWindow.Show();
            this.Close();
        }

        private void ButtonWork(string command)
        {
            if (User != null)
            {
                SendCommand(command);

                try
                {
                    client = new TcpClient("127.0.0.1", Port);
                    using (stream = client.GetStream())
                    {
                        PackData pack = new PackData { User = User, Video = Video };
                        XmlSerializer xml = new XmlSerializer(pack.GetType());
                        xml.Serialize(stream, pack);
                    }
                }
                catch (ApplicationException ex)
                {
                    mainWindow.LogWork(ex.Message);
                }
                finally { client.Close(); }

            }
        }
        private void btnAddToFavorites_Click(object sender, RoutedEventArgs e) => Task.Run(() => ButtonWork("AddVideo"));
        private void btnSendTelegAsSong_Click(object sender, RoutedEventArgs e) => Task.Run(() => SendAudioToTelegramAsync());

        private async void SendAudioToTelegramAsync()
        {
            if (user != null)
            {
                string link = Video.Adress;
                var youtube = YouTube.Default;
                var video = await youtube.GetVideoAsync(link);
                string file = video.FullName;

                File.WriteAllBytes(file, await video.GetBytesAsync());
                var convertAudio = new FFMpegConverter();
                convertAudio.ConvertMedia(file, file.Substring(0, file.IndexOf(".")) + ".mp3", "mp3");

                Bot.SendAudio(User.Token, file);

                
                Dispatcher.Invoke(() =>
                {
                    Info = "Audio sended to personal telegram account";
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Info = "Please Log In";
                });
            }
        }

        private void btnSendTeleg_Click(object sender, RoutedEventArgs e) => Task.Run(() => SendVideoToTelegramAsync());

        private async void SendVideoToTelegramAsync()
        {
            if (User != null)
            {
                string link = Video.Adress;
                var youtube = YouTube.Default;
                var video = await youtube.GetVideoAsync(link);
                string file = video.FullName;

                File.WriteAllBytes(file, await video.GetBytesAsync());

                Bot.SendVideo(User.Token, file);

                File.Delete(file);

                Dispatcher.Invoke(() =>
                {
                    Info = "Video sended to personal telegram account";
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Info = "Please Log In";
                });
            }
        }

        private void btnSaveAsSong_Click(object sender, RoutedEventArgs e) => Task.Run(() => ExtractAudioAsync());
        private void btnFDownload_Click(object sender, RoutedEventArgs e) => Task.Run(() => ExtractVideoAsync());

        private async void ExtractVideoAsync()
        {
            string path = "";
            Dispatcher.Invoke(() =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Choose a path" })
                {
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = fbd.SelectedPath;
                    }
                }
            });

            string link = Video.Adress;
            var youtube = YouTube.Default;
            var video = await youtube.GetVideoAsync(link);

            File.WriteAllBytes(path + @"\" + video.FullName, await video.GetBytesAsync());

            Dispatcher.Invoke(() =>
                {
                    Info = "Video downloading completed";
                });
        }

        private async void ExtractAudioAsync()
        {
            string path = "";
            Dispatcher.Invoke(() =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Choose a path" })
                {
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = fbd.SelectedPath;
                    }
                }
            });

            string link = Video.Adress;
            var youtube = YouTube.Default;
            var video = await youtube.GetVideoAsync(link);
            string file = path + @"\" + video.FullName;

            File.WriteAllBytes(file, await video.GetBytesAsync());
            var convertAudio = new FFMpegConverter();
            convertAudio.ConvertMedia(file, file.Substring(0, file.IndexOf(".")) + ".mp3", "mp3");
            File.Move(file, path + @"\" + Video.VideoTitle + ".mp3");

            Dispatcher.Invoke(() =>
            {
                Info = "Audio downloading completed";
            });
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
    }
}
