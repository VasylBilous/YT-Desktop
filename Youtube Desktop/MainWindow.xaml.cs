using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Youtube_Desktop.data;

namespace Youtube_Desktop
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string key = "AIzaSyCP3Q-n66YYg_z4sFPskirl_AaUCTxEwyw";
        private string path;
        private string search;
        private Video selected;
        private User user;

        public string Path { get => path; set { path = value; NotifyUI(); } }
        public string Search { get => search; set { search = value; NotifyUI(); } }
        public ObservableCollection<Video> Videos { get; set; } = new ObservableCollection<Video>();
        public Video Selected { get => selected; set { selected = value; Path = Selected.Adress; NotifyUI(); } }
        public User User
        {
            get => user; set
            {
                user = value;
                btnFavorites.Visibility = Visibility.Visible;
                btnSettings.Visibility = Visibility.Visible;
                NotifyUI();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Button_Click(null, null);
        }

        private void VideoField_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VideoWindow videoWindow = new VideoWindow(this);
            this.Hide();
            videoWindow.Show();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(this);
            this.Hide();
            loginWindow.Show();
        }

        private async void UpdateVideosAsync(string query)
        {
            Videos.Clear();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = key });
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 48;
            var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var r in searchListResponse.Items)
                Videos.Add(new Video(r));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyUI([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateVideosAsync("");
            Search = "";
        }

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            UpdateVideosAsync(Search);
            Search = "";
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void EnterCommand(object sender, ExecutedRoutedEventArgs e) => btnSearch_Click(null, null);
        public void EscapeCommand(object sender, ExecutedRoutedEventArgs e) => btnQuit_Click(null, null);

        private void btnFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (User != null)
            {
                FavoritesWindow videoWindow = new FavoritesWindow(this);
                this.Hide();
                videoWindow.Show();
            }
        }

        public void LogWork(string text)
        {
            using (StreamWriter writer = new StreamWriter("../../log/YoutubeApp Log.txt", true))
            {
                writer.WriteLine(DateTime.Now + "\t" + text);
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow videoWindow = new SettingsWindow(this);
            this.Hide();
            videoWindow.Show();
        }
    }
}
