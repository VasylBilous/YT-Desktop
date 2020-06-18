using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Youtube_Desktop
{
    /// <summary>
    /// Interaction logic for CodeAuthenWindow.xaml
    /// </summary>
    public partial class CodeAuthenWindow : Window, INotifyPropertyChanged
    {
        private static Random random = new Random();
        private static LoginWindow loginWindow = null;
        private string email;
        private string genetaredCode;
        private string code;
        public string Code { get => code; set { code = value; NotifyUI(); } }
        public CodeAuthenWindow(LoginWindow log, string em)
        {
            InitializeComponent();
            DataContext = this;

            loginWindow = log;
            email = em;
            SendSecurityCode();
        }

       private void SendSecurityCode()
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("mkads2019@gmail.com", "MK0mk0MK0");

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("mkads2019@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Security Code from Youtube Desktop";
            genetaredCode = GetCode();
            mail.Body = $"Hi there from the Youtube Desktope \n Your security code: {genetaredCode}";
            client.Send(mail);
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (Code.Equals(genetaredCode))
                loginWindow.GoodRegistration();
            else
                loginWindow.BadRegistration();
            
            this.Close();
        }

        private string GetCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

          return new String(stringChars);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyUI([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
