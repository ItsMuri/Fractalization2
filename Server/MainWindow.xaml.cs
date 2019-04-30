using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FractalLibrary;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace Server
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<Bitmap> bitmapList = new List<Bitmap>();
        private Dictionary<int, Bitmap> bitmapDict = new Dictionary<int, Bitmap>();
        private readonly List<TcpClient> listConnectedClients = new List<TcpClient>();
        private TcpListener listener;
        private Point origin;
        private Point start;
        private Bitmap newImage = new Bitmap(400, 400);
        private int offset;
        //TransformGroup group = new TransformGroup();
        //ScaleTransform st = new ScaleTransform();
        //TranslateTransform tt = new TranslateTransform();

        public MainWindow()
        {
            InitializeComponent();

            BorderImage.ClipToBounds = true;

            //group.Children.Add(st);
            //group.Children.Add(tt);
            //imageFraktal.RenderTransform = group;
        }

        private void ZeichneFraktal()
        {
            // Bitmap zum Zeichnen des Fraktals verwenden!
            //Referenz auf Video YT in liked Liste!


            var cntInterations = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));
            var ft = new FraktalTask();
            // in ft werden die verschiedenen Koordinaten 


            //Versuche das Fraktal in 2 Sektoren zu unterteilen!!!
            var bm = new Bitmap(Convert.ToInt32(imageFraktal.Width), Convert.ToInt32(imageFraktal.Height));
            var lowerBm = new Bitmap(Convert.ToInt32(imageFraktal.Width / 2),
                Convert.ToInt32(imageFraktal.Height / 2));
            //Haben jetzt zwei Bereiche, lowerBm ist der untere Bereich der Bitmap
            //also in diesem Fall 200 x 200 bei einem Original von 400 x 400
            //Idee: Färbe zuerst die 400 x 400 Fläche und danach die 200 x 200 Fläche


            for (var x = 0; x < imageFraktal.Width / 2; x++)
                for (var y = 0; y < imageFraktal.Height; y++)
                {
                    var a = (x - imageFraktal.Width / 2) / (imageFraktal.Width / 4);
                    var b = (y - imageFraktal.Height / 2) / (imageFraktal.Height / 4);
                    var c = new Complex(a, b);
                    var z = new Complex(0, 0);
                    var it = 0;
                    do
                    {
                        it++;
                        z.Square();
                        z.Add(c);
                        if (z.Magnitude() > 2.0)
                            break;
                    } while (it < cntInterations);


                    //bm.SetPixel(x, y, it < 50 ? Color.Black : Color.Blue);
                    bm.SetPixel(x, y, it < cntInterations ? Color.Aquamarine : Color.Red);
                    //lowerBm.SetPixel(x,y, it < cntInterations ? Color.Yellow : Color.Blue);
                    //Mehrere Farben so anzeigen lassen, funktioniert so nicht!!!
                }


            imageFraktal.Source = BitmapToImageSource(bm);

            for (var x = imageFraktal.Width / 2; x < imageFraktal.Width; x++)
                for (var y = 0; y < imageFraktal.Height; y++)
                {
                    var a = (x - imageFraktal.Width / 2) / (imageFraktal.Width / 4);
                    var b = (y - imageFraktal.Height / 2) / (imageFraktal.Height / 4);
                    var c = new Complex(a, b);
                    var z = new Complex(0, 0);
                    var it = 0;
                    do
                    {
                        it++;
                        z.Square();
                        z.Add(c);
                        if (z.Magnitude() > 2.0)
                            break;
                    } while (it < cntInterations);

                    //bm.SetPixel(x, y, it < 50 ? Color.Black : Color.Blue);
                    bm.SetPixel(Convert.ToInt32(x), y, it < cntInterations ? Color.Aquamarine : Color.Red);
                    //lowerBm.SetPixel(x,y, it < cntInterations ? Color.Yellow : Color.Blue);
                    //Mehrere Farben so anzeigen lassen, funktioniert so nicht!!!
                }

            imageFraktal.Source = BitmapToImageSource(bm);
        }

        /*
        private Task Empfangen()
        {
            //Hier wird dann später empfangen
            
        }
        */

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listener = new TcpListener(IPAddress.Loopback, 2222);
            listener.Start();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var iterationsCount = Convert.ToInt32(tbIterations.Text);
            var myFraktal = new PropsOfFractal(iterationsCount)
            {
                ImgWidth = imageFraktal.Width,
                ImgHeight = imageFraktal.Height
            };

            Task.Run(() => Senden(myFraktal));
        }

        private void Senden(PropsOfFractal myFraktal)
        {
            int Id = 0;
            bitmapDict = new Dictionary<int, Bitmap>();

            while (true)
            {
                var mySender = listener.AcceptTcpClient();
                listConnectedClients.Add(mySender);
                Dispatcher.Invoke(() => labelComputerAvailable.Content = listConnectedClients.Count);

                Task.Factory.StartNew((object state) =>
                {
                    //if (bitmapDict.Count.ToString() !=
                    //    Dispatcher.Invoke(() => CmbClientQuantity.Text))
                    //{
                    int internalID = (int)state;
                    var fIdClone = myFraktal.Clone() as PropsOfFractal;
                    fIdClone.Id = internalID;
                    fIdClone.ClientCount = int.Parse(Dispatcher.Invoke(() => CmbClientQuantity.Text));
                    //}
                    //var selectedItem = int.Parse(Dispatcher.Invoke(() => CmbClientQuantity.Text));

                    var netStream = mySender.GetStream();
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    serializer.WriteObject(netStream, fIdClone);

                    mySender.Client.Shutdown(SocketShutdown.Send);

                    var bitmSerializer = new DataContractSerializer(typeof(Bitmap));
                    var verarbeiteteDaten = (Bitmap)bitmSerializer.ReadObject(netStream);

                    netStream.Close();
                    mySender.Close();

                    //bitmapList.Add(verarbeiteteDaten);
                    bitmapDict.Add(internalID, verarbeiteteDaten);

                    //if ((internalID + 1).ToString() == myFraktal.clientCount.ToString())
                    verarbeiteteDaten.Save($"bitmap{internalID}.jpg");
                    ////FraktalAnzeigenOld(bitmapDict);
                    FraktalAnzeigen(internalID, verarbeiteteDaten);
                }, Id++);
            }
        }

        //public Task SendTask(PropsOfFractal myFraktal, TcpClient mySender)
        //{


        //    return Task.CompletedTask;
        //}

        private void FraktalAnzeigen(int internalId, Bitmap bitm)
        {
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(bitm, new Rectangle(bitm.Width * internalId, 0, bitm.Width, bitm.Height));
            }

            imageFraktal.Dispatcher.Invoke(() => imageFraktal.Source = BitmapToImageSource(newImage));


            //for (var x = 0; x < width / 2; x++)
            //    for (var y = 0; y < height; y++)
            //        newImage.SetPixel(x, y, bitmList[0].GetPixel(x, y));
            //for (var x = width / 2; x < width; x++)
            //    for (var y = 0; y < height; y++)
            //        newImage.SetPixel(x, y, bitmList[1].GetPixel(x, y));
            //}

            //BitmapImage bmi = BitmapToImageSource(verabeiteteDaten);
        }

        /*private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                listener = new TcpListener(IPAddress.Loopback, 5566);
                listener.Start();
            });
        }*/



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



        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void ImageFraktal_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform)((TransformGroup)imageFraktal.RenderTransform).Children.First(tr =>
              tr is ScaleTransform);
            var zoom = e.Delta > 0 ? .2 : -.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }

        private void ImageFraktal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imageFraktal.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)imageFraktal.RenderTransform).Children.First(tr =>
              tr is TranslateTransform);

            start = e.GetPosition(BorderImage);
            origin = new Point(tt.X, tt.Y);
        }

        private void ImageFraktal_MouseMove(object sender, MouseEventArgs e)
        {
            if (imageFraktal.IsMouseCaptured)
            {
                var tt = (TranslateTransform)((TransformGroup)imageFraktal.RenderTransform).Children.First(tr =>
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