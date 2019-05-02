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
using Point = System.Windows.Point;
using Color = System.Drawing.Color;

namespace Server
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Bitmap> bitmapList = new List<Bitmap>();
        private readonly List<TcpClient> listConnectedClients = new List<TcpClient>();

        private TcpListener clientListener;

        //private TcpListener listener;
        private Point origin;

        private Point start;

        private TcpListener listener;

        public MainWindow()
        {
            InitializeComponent();

            BorderImage.ClipToBounds = true;

            var localep = new IPEndPoint(IPAddress.Loopback, 3333);
            listener = new TcpListener(localep);
            listener.Start();

            Task T = new Task(() =>
            {
                //Hello();
                Connection();
            });
            T.Start();
            //Zeichnen lassen
            //Vielleicht kann man das mit WriteableBitmap zeichnen lassen...
        }

        private void Hello()
        {
            // Console.WriteLine("---Server---");
            var localep = new IPEndPoint(IPAddress.Loopback, 0);
            TcpClient client = new TcpClient(localep);

            var remotep = new IPEndPoint(IPAddress.Loopback, 6666);
            try
            {
                Thread.Sleep(10000);
                client.Connect(remotep);
                try
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        while (true)
                        {
                            using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                            {
                                int itanzahl = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));

                                writer.WriteLine("Hello." + itanzahl.ToString());
                            }
                            using (var reader = new StreamReader(stream, Encoding.ASCII, true, 4096, leaveOpen: true))
                            {
                                string response = reader.ReadLine();
                                while (response != "Got it")
                                {
                                }
                            }
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Der BackupServer ist nicht mehr erreichbar!" + e);
                }
                client.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Kein BackupServer zur Verfügung!" + e);
            }
        }

        private void Connection()
        {
            while (true)
            {
                while(Success() == false)
                {
                    Success();
                }
            }
        }

        private bool Success()
        {
            var localep = new IPEndPoint(IPAddress.Loopback, 0);
            TcpClient client = new TcpClient(localep);

            var remotep = new IPEndPoint(IPAddress.Loopback, 6666);
            try
            {
                Thread.Sleep(10000);
                client.Connect(remotep);
                try
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        while (true)
                        {
                            using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                            {
                                int itanzahl = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));
                                writer.WriteLine("Hello." + itanzahl.ToString());
                            }
                            using (var reader = new StreamReader(stream, Encoding.ASCII, true, 4096, leaveOpen: true))
                            {
                                string response = reader.ReadLine();
                                while (response != "Got it")
                                {
                                }
                            }
                            Thread.Sleep(2000);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Der BackupServer ist nicht mehr erreichbar!" + e);
                    client.Close();
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Kein BackupServer zur Verfügung!" + e);
                client.Close();
                return false;
            }
        }


        private void ZeichneFraktal()
        {
            // Bitmap zum Zeichnen des Fraktals verwenden!
            //Referenz auf Video YT in liked Liste!


            int cntInterations = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));
            FraktalTask ft = new FraktalTask();
            // in ft werden die verschiedenen Koordinaten 


            //Versuche das Fraktal in 2 Sektoren zu unterteilen!!!
            Bitmap bm = new Bitmap(Convert.ToInt32(imageFraktal.Width), Convert.ToInt32(imageFraktal.Height));
            Bitmap lowerBm = new Bitmap(Convert.ToInt32(imageFraktal.Width / 2),
                Convert.ToInt32(imageFraktal.Height / 2));
            //Haben jetzt zwei Bereiche, lowerBm ist der untere Bereich der Bitmap
            //also in diesem Fall 200 x 200 bei einem Original von 400 x 400
            //Idee: Färbe zuerst die 400 x 400 Fläche und danach die 200 x 200 Fläche


            for (int x = 0; x < imageFraktal.Width; x++)
            {
                for (int y = 0; y < imageFraktal.Height; y++)
                {
                    double a = (double) (x - (imageFraktal.Width / 2)) / (double) (imageFraktal.Width / 4);
                    double b = (double) (y - (imageFraktal.Height / 2)) / (double) (imageFraktal.Height / 4);
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
            //TcpClient mySender = new TcpClient();
            //mySender.Connect(IPAddress.Loopback, 5566);

            while (myFraktal.ID < 2)
            {
                var mySender = listener.AcceptTcpClient();
                listConnectedClients.Add(mySender);
                Dispatcher.Invoke(() => labelComputerAvailable.Content = listConnectedClients.Count);

                myFraktal.ID += 1;

                var netStream = mySender.GetStream();
                var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                serializer.WriteObject(netStream, myFraktal);

                mySender.Client.Shutdown(SocketShutdown.Send);

                var bitmSerializer = new DataContractSerializer(typeof(Bitmap));
                var verarbeiteteDaten = (Bitmap) bitmSerializer.ReadObject(netStream);

                netStream.Close();
                mySender.Close();

                bitmapList.Add(verarbeiteteDaten);

                if (bitmapList.Count == 2)
                    FraktalAnzeigen(bitmapList, myFraktal);
            }


            /*
            Task empfangen = new Task(() =>
            {
                //var serializer = new DataContractSerializer(typeof(FraktalSrv));

                //NetworkStream netStream = new NetworkStream(new Socket(SocketType.Stream, ProtocolType.Tcp));
            });
            empfangen.Start();
            */

            //Ich rufe als Abschluss nun also die Empfangsmethode auf.


            /*
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
            */
        }

        private void FraktalAnzeigen(List<Bitmap> bitmapList, PropsOfFractal myFraktal)
        {
            //BitmapImage bmi = BitmapToImageSource(verabeiteteDaten);
            //imageFraktal.Dispatcher.Invoke(() => imageFraktal.Source = BitmapToImageSource(verabeiteteDaten));
            //MessageBox.Show("Fertig");

            //foreach (var item in bitmapList)
            //{
            var width = Convert.ToInt32(myFraktal.imgWidth);
            var height = Convert.ToInt32(myFraktal.imgHeight);
            var newImage = new Bitmap(width, height);

            for (var x = 0; x < width / 2; x++)
            for (var y = 0; y < height; y++)
                newImage.SetPixel(x, y, bitmapList[0].GetPixel(x, y));
            for (var x = width / 2; x < width; x++)
            for (var y = 0; y < height; y++)
                newImage.SetPixel(x, y, bitmapList[1].GetPixel(x, y));
            //}

            //BitmapImage bmi = BitmapToImageSource(verabeiteteDaten);
            imageFraktal.Dispatcher.Invoke(() => imageFraktal.Source = BitmapToImageSource(newImage));
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

            Task senden = new Task(() => { Senden(myFraktal); });
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

        private void ImageFraktal_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform) ((TransformGroup) imageFraktal.RenderTransform).Children.First(tr =>
                tr is ScaleTransform);
            var zoom = e.Delta > 0 ? .2 : -.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }

        private void ImageFraktal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imageFraktal.CaptureMouse();
            var tt = (TranslateTransform) ((TransformGroup) imageFraktal.RenderTransform).Children.First(tr =>
                tr is TranslateTransform);

            start = e.GetPosition(BorderImage);
            origin = new Point(tt.X, tt.Y);
        }

        private void ImageFraktal_MouseMove(object sender, MouseEventArgs e)
        {
            if (imageFraktal.IsMouseCaptured)
            {
                var tt = (TranslateTransform) ((TransformGroup) imageFraktal.RenderTransform).Children.First(tr =>
                    tr is TranslateTransform);

                var v = start - e.GetPosition(BorderImage);
                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;
            }
        }

        private void ImageFraktal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageFraktal.ReleaseMouseCapture();
        }
    }
}