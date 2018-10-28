using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
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

namespace Server
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener listener;

        public MainWindow()
        {
            InitializeComponent();
            int iterationsCount = Convert.ToInt32(tbIterations.Text);
            FraktalSrv myFraktal = new FraktalSrv(iterationsCount);
            //Koordinaten versenden!
            double[] xCoordinates = new double[5];
            for (int i = 0; i < 5; i++)
            {
                xCoordinates[i] = -2.0 + i;
            }
            myFraktal.KoordinatenX = xCoordinates; //X Koordinaten gesetzt!
            double[] ycoordinates = new double[2];
            double imaginaryNumber = Math.Sqrt(-1);
            ycoordinates[0] = Math.Sqrt(-imaginaryNumber);
            ycoordinates[1] = Math.Sqrt(imaginaryNumber);
            myFraktal.KoordinatenY = ycoordinates; //Y Koordinaten gesetzt!

            Task senden = new Task(() => { Senden(myFraktal); });
            //senden.Start();
        }


        private void Senden(FraktalSrv myFraktal)
        {
            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                //Hier werden nun die Informationen gesendet

                using (NetworkStream netStream = tcpClient.GetStream())
                {
                    var serializer = new DataContractSerializer(typeof(FraktalSrv));
                    serializer.WriteObject(netStream, myFraktal);
                    netStream.Position = 0;

                    tcpClient.Client.Shutdown(SocketShutdown.Send);


                    var ser = new DataContractSerializer(typeof(FraktalSrv));
                    FraktalSrv verabeiteteDaten = (FraktalSrv) ser.ReadObject(netStream);
                }

                //Task empfangen = new Task(() => { Empfangen(); });
                //empfangen.Start();
                //Ich rufe als Abschluss nun also die Empfangsmethode auf.
            }
        }

        private void Empfangen()
        {
            //Hier wird dann später empfangen
            var serializer = new DataContractSerializer(typeof(FraktalSrv));
            NetworkStream netStream = new NetworkStream(new Socket(SocketType.Stream, ProtocolType.Tcp));
            FraktalSrv verabeiteteDaten = (FraktalSrv) serializer.ReadObject(netStream);
        }


        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                listener = new TcpListener(IPAddress.Loopback, 5566);
                listener.Start();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Task t = CalculateTask();
        }

        /*private async Task CalculateTask()
        {
            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    while (true)
                    {
                        using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                        {
                            string str = Convert.ToString("Connection established");
                            writer.WriteLine(str);
                        }

                        using (var reader =
                            new StreamReader(stream, Encoding.ASCII, true, 4096, leaveOpen: true))
                        {
                            string answer = reader.ReadLine();
                            MessageBox.Show($"{answer}");
                        }
                    }
                }
            }
        }*/

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Dieser Code wird erst verwendet wenn wir erste Testclients haben!

            /*
            int countAvailablePcS = 0;
            string[] availablePcS = new string[100];
            bool isOn;
            TcpClient expeditionClient = new TcpClient();

            for (int i = 5460; i <= 5560; i++)
            {
                expeditionClient.Connect(IPAddress.Loopback,i);
                isOn = expeditionClient.Client.Connected;
                if (isOn == true)
                {
                    countAvailablePcS++;
                    labelComputerAvailable.Content = countAvailablePcS;
                    expeditionClient.Close();
                }
            }*/
        }
    }
}