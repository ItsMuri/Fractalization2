using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Login
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static string ApplicationName = "Fractalization";
        public static string ClientId = "629779148724-mlaq83snjlouftbahoqvb43koq5l8gnl.apps.googleusercontent.com";
        public static string ClientSecret = "94X2nNKMY9qTTcj40lJoIpQD";
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string[] Scopes =
        {
            GmailService.Scope.GmailCompose,
            GmailService.Scope.GmailSend
        };

        public static UserCredential GetUserCredential(out string error)
        {
            UserCredential credential = null;
            error = string.Empty;

            try
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    },
                    Scopes,
                    Environment.UserName,
                    CancellationToken.None,
                    new FileDataStore("Google Oauth2")).Result;
            }
            catch(Exception ex)
            {
                credential = null;
                error = "Failed to UserCredential Initialization: " + ex.ToString();
            }
            return credential;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string credentialError = string.Empty;
            string refreshToken = string.Empty;

            UserCredential credential = GetUserCredential(out credentialError);
            if(credential != null && string.IsNullOrWhiteSpace(credentialError))
            {
                refreshToken = credential.Token.RefreshToken;
            }

            Server.MainWindow mainWindow = new Server.MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
