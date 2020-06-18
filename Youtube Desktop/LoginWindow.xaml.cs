using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;
using Youtube_Desktop.data;

namespace Youtube_Desktop
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private MainWindow m = null;
        private const int Port = 3030;
        private const int Port2 = 3033;
        private NetworkStream stream = null;
        private TcpClient client = null;
        private User user = new User();

        private string login;
        private string info;
        private bool authent;

        public string Login { get => login; set { login = value; NotifyUI(); } }
        public string Password { private get; set; }
        public string Info { get => info; set { info = value; NotifyUI(); } }
        public bool Authent { get => authent; set { authent = value; NotifyUI(); } }

        public LoginWindow(MainWindow main)
        {
            InitializeComponent();
            DataContext = this;
            m = main;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!RegexLoginCheck(Login))
            {
                Info = "Email adress is not correct!";
                Login = "";
                tbPassword.Password = "";
                return;
            }

            if (!DbLoginCheck(Login))
            {
                Info = "Email adress already exist. Just Log In!";
                Login = "";
                tbPassword.Password = "";
                return;
            }

            CodeAuthen(Login);
        }

        public void BadRegistration()
        {
            this.Show();
            Info = "Code is not correct!";
            Login = "";
            Password = "";
        }

        public void GoodRegistration()
        {
            user = new User { Login = Login, Password = Password };
            SaveToDb(user);
            m.User = user;
            m.Show();
            this.Close();
        }

        private void SaveToDb(User user)
        {
            SendCommand("SaveUser");
            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(user.GetType());
                    xml.Serialize(stream, user);
                }
            }
            catch (ApplicationException ex) { m.LogWork(ex.Message); }
            finally { client.Close(); }
        }

        private void CodeAuthen(string email)
        {
            CodeAuthenWindow codeAuthen = new CodeAuthenWindow(this, email);
            this.Hide();
            codeAuthen.Show();
        }

        private bool DbLoginCheck(string login)
        {
            SendCommand("LoginCheck");
            SendCommand(login);

            TcpListener serv = null;
            bool res = false;
            try
            {
                serv = new TcpListener(IPAddress.Parse("127.0.0.1"), Port2);
                serv.Start();
                client = serv.AcceptTcpClient();

                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(typeof(bool));
                    res = (bool)xml.Deserialize(stream);
                }
            }
            catch (ApplicationException ex) { m.LogWork(ex.Message); }
            finally { client.Close(); serv.Stop(); }
            return res;
        }
        private bool DbUserLogin()
        {
            SendCommand("UserLogin");
            try
            {
                client = new TcpClient("127.0.0.1", Port);
                using (stream = client.GetStream())
                {
                    user = new User { Login = Login, Password = Password };
                    XmlSerializer xml = new XmlSerializer(user.GetType());
                    xml.Serialize(stream, user);
                }
            }
            catch (ApplicationException ex) { m.LogWork(ex.Message); }
            finally { client.Close(); }

            TcpListener serv = null;
            bool res = false;
            try
            {
                serv = new TcpListener(IPAddress.Parse("127.0.0.1"), Port2);
                serv.Start();
                client = serv.AcceptTcpClient();
                using (stream = client.GetStream())
                {
                    XmlSerializer xml = new XmlSerializer(typeof(LoginPackage));
                    var package = (LoginPackage)xml.Deserialize(stream);
                    res = package.Res;
                    user = new User { Login = package.User.Login, Password = package.User.Password, Token = package.User.Token };
                }
            }
            catch (ApplicationException ex) { m.LogWork(ex.Message); }
            finally { client.Close(); serv.Stop(); }
            return res;
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
            catch (ApplicationException ex) { m.LogWork(ex.Message); }
            finally { client.Close(); }
        }

        private bool RegexLoginCheck(string login)
        {
            string emailPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";

            if (Regex.IsMatch(login, emailPattern, RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyUI([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e) => Password = tbPassword.Password;

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (DbUserLogin())
            {
                m.User = user;
                m.Show();
                this.Close();
            }
            else
            {
                Info = "Email or password is not correct!";
                Login = "";
                tbPassword.Password = "";
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            m.Show();
            this.Close();
        }
    }
}
