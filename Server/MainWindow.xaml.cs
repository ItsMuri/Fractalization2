using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SerializedFraktal;
using Color = System.Drawing.Color;

namespace Server
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private TcpListener listener;

        public MainWindow()
        {
            InitializeComponent();
            
            //Zeichnen lassen
            //Vielleicht kann man das mit WriteableBitmap zeichnen lassen...

        }

        private void ZeichneFraktal()
        {
            // Bitmap zum Zeichnen des Fraktals verwenden!
            //Referenz auf Video YT in liked Liste!


            int cntInterations = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));
            FraktalTask ft = new FraktalTask();
            // in ft werden die verschiedenen Koordinaten 
            

                //Versuche das Fraktal in 2 Sektoren zu unterteilen!!!
                Bitmap bm = new Bitmap(Convert.ToInt32(imageFraktal.Width),Convert.ToInt32(imageFraktal.Height));
                Bitmap lowerBm = new Bitmap(Convert.ToInt32(imageFraktal.Width/2), Convert.ToInt32(imageFraktal.Height/2));
                //Haben jetzt zwei Bereiche, lowerBm ist der untere Bereich der Bitmap
                //also in diesem Fall 200 x 200 bei einem Original von 400 x 400
                //Idee: Färbe zuerst die 400 x 400 Fläche und danach die 200 x 200 Fläche
        

        for (int x = 0; x < imageFraktal.Width; x++)
            {
                for (int y = 0; y < imageFraktal.Height; y++)
                {
                    double a = (double)(x - (imageFraktal.Width / 2)) / (double)(imageFraktal.Width / 4);
                    double b = (double)(y - (imageFraktal.Height / 2)) / (double)(imageFraktal.Height / 4);
                    Complex c = new Complex(a, b);
                    Complex z = new Complex(0, 0);
                    int it = 0;
                    do
                    {
                        it++;
                        z.Square();
                        z.Add(c);
                        if (z.Magnitude() > 2.0)
                        {
                            break;
                        }
                    } while (it < cntInterations);

                    //bm.SetPixel(x, y, it < 50 ? Color.Black : Color.Blue);
                    bm.SetPixel(x, y, it < cntInterations ? Color.Aquamarine : Color.Red);
                    //lowerBm.SetPixel(x,y, it < cntInterations ? Color.Yellow : Color.Blue);
                    //Mehrere Farben so anzeigen lassen, funktioniert so nicht!!!

                }
            }
            


            BitmapImage bmi = BitmapToImageSource(bm);

            imageFraktal.Source = bmi;
            
        }

        /*
        private Task Empfangen()
        {
            //Hier wird dann später empfangen
            
        }
        */

        private void Senden(PropsOfFractal myFraktal)
        {
            //Hier werden nun die Informationen gesendet
            TcpClient mySender = new TcpClient();
            mySender.Connect(IPAddress.Loopback,5566);
            NetworkStream netStream = mySender.GetStream();
            var serializer = new DataContractSerializer(typeof(PropsOfFractal));
            serializer.WriteObject(netStream, myFraktal);
            mySender.Client.Shutdown(SocketShutdown.Send);
            var BitmSerializer = new DataContractSerializer(typeof(Bitmap));
            Bitmap verabeiteteDaten = (Bitmap)BitmSerializer.ReadObject(netStream);
            netStream.Close();
            mySender.Close();

            FraktalAnzeigen(verabeiteteDaten);
            

            /*
            Task empfangen = new Task(() =>
            {
                //var serializer = new DataContractSerializer(typeof(FraktalSrv));

                //NetworkStream netStream = new NetworkStream(new Socket(SocketType.Stream, ProtocolType.Tcp));
            });
            empfangen.Start();
            */

            //Ich rufe als Abschluss nun also die Empfangsmethode auf.
        }

        private void FraktalAnzeigen(Bitmap verabeiteteDaten)
        {
            //BitmapImage bmi = BitmapToImageSource(verabeiteteDaten);
            imageFraktal.Dispatcher.Invoke(() => imageFraktal.Source = BitmapToImageSource(verabeiteteDaten));
        }

        /*private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                listener = new TcpListener(IPAddress.Loopback, 5566);
                listener.Start();
            });
        }*/

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Task t = CalculateTask();
            //ZeichneFraktal();


            int iterationsCount = Convert.ToInt32(tbIterations.Text);
            SerializedFraktal.PropsOfFractal myFraktal = new PropsOfFractal(iterationsCount);
            myFraktal.imgWidth = imageFraktal.Width;
            myFraktal.imgHeight = imageFraktal.Height;
            //FraktalSrv myFraktal = new FraktalSrv(iterationsCount);
            
            /*double[] xCoordinates = new double[5];
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
            */

            Task senden = new Task(() =>
            {
                Senden(myFraktal);
            });
            senden.Start();
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

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}