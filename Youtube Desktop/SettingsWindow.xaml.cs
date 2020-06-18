using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Youtube_Desktop.data;

namespace Youtube_Desktop
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;
        private const int Port = 3030;
        private NetworkStream stream = null;
        private TcpClient client = null;

        public User User { get; set; }

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            User = mainWindow.User;
            DataContext = this;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            tbPass.Visibility = Visibility.Visible;
            tbPassword.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SendUserData());
            mainWindow.Show();
            this.Hide();
        }
        private void SendUserData()
        {
            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    string command = "UpdateUser";
                    XmlSerializer xml = new XmlSerializer(command.GetType());
                    xml.Serialize(stream, command);
                }
            }
            catch (ApplicationException ex)
            {
                mainWindow.LogWork(ex.Message);
            }
            finally { client.Close(); }

            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    User[] users = new User[2];
                    users[0] = User;
                    string tok = "", pas = "";
                    Dispatcher.Invoke(() =>
                    {
                        tok = tbToken.Text;
                        pas = tbPassword.Password;
                    });
                    User.Password = pas;
                    User.Token = tok;
                    users[1] = User;
                    XmlSerializer xml = new XmlSerializer(users.GetType());
                    xml.Serialize(stream, users);
                }
            }
            catch (ApplicationException ex)
            {
                mainWindow.LogWork(ex.Message);
            }
            finally { client.Close(); }

            Dispatcher.Invoke(() => this.Close());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Show();
            this.Close();
        }
    }
}
