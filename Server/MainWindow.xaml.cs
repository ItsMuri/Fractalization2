using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

        private System.Windows.Shapes.Rectangle selection = new System.Windows.Shapes.Rectangle()
        {
            Stroke = System.Windows.Media.Brushes.Black,
            StrokeThickness = 1,
            Visibility = Visibility.Collapsed
        };
        private Point mouseDownPos;
        //TransformGroup group = new TransformGroup();
        //ScaleTransform st = new ScaleTransform();
        //TranslateTransform tt = new TranslateTransform();

        public MainWindow()
        {
            InitializeComponent();

            var ipfromFile = File.ReadAllLines(@"config.cfg");
            IPAddress.TryParse(ipfromFile[0], out IPAddress ipServer);
            IPAddress.TryParse(ipfromFile[1], out IPAddress ipBackup);

            var localep = new IPEndPoint(ipServer, 3333);
            listener = new TcpListener(localep);
            listener.Start();

            Task T = new Task(() =>
            {
                Connection();
            });
            T.Start();

            BorderImage.ClipToBounds = true;

            //group.Children.Add(st);
            //group.Children.Add(tt);
            //imageFraktal.RenderTransform = group;
        }

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
            int counter = 0;

            while (true)
            {
                var mySender = listener.AcceptTcpClient();
                listConnectedClients.Add(mySender);
                Dispatcher.Invoke(() => labelComputerAvailable.Content = listConnectedClients.Count);
                counter++;

                Task.Factory.StartNew((object state) =>
                {
                    AesCryptoServiceProvider cryptic = new AesCryptoServiceProvider();
                    string key = "Tg6VU5ZzKjNrR0UoYrVVgPdZafNtLT4XSwyWQRvna1w=";
                    string IV = "NjjwRHuRPEgLAT0qD+0UaQ==";

                    byte[] keybyte = Convert.FromBase64String(key);
                    byte[] ivbyte = Convert.FromBase64String(IV);


                    cryptic.Key = keybyte;
                    cryptic.IV = ivbyte;

                    //cryptic.GenerateKey();
                    //cryptic.GenerateIV();

                    //string k = Convert.ToBase64String(cryptic.Key);
                    //string iv = Convert.ToBase64String(cryptic.IV);


                    int internalID = (int)state;
                    var fIdClone = myFraktal.Clone() as PropsOfFractal;
                    fIdClone.Id = internalID;
                    fIdClone.ClientCount = int.Parse(Dispatcher.Invoke(() => CmbClientQuantity.Text));
                    //}
                    //var selectedItem = int.Parse(Dispatcher.Invoke(() => CmbClientQuantity.Text));

                    var netStream = mySender.GetStream();
                    CryptoStream encryptStream = new CryptoStream(netStream, cryptic.CreateEncryptor(), CryptoStreamMode.Write);
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    serializer.WriteObject(encryptStream, fIdClone);

                    encryptStream.FlushFinalBlock();

                    mySender.Client.Shutdown(SocketShutdown.Send);

                    CryptoStream decryptStream = new CryptoStream(netStream, cryptic.CreateDecryptor(), CryptoStreamMode.Read);
                    var bitmSerializer = new DataContractSerializer(typeof(Bitmap));
                    var verarbeiteteDaten = (Bitmap)bitmSerializer.ReadObject(decryptStream);

                    decryptStream.Close();
                    netStream.Close();
                    mySender.Close();
                    
                    verarbeiteteDaten.Save($"bitmap{internalID}.jpg");

                    FraktalAnzeigen(internalID, verarbeiteteDaten);
                    
                }, Id++);

                if (counter == Convert.ToInt32(Dispatcher.Invoke(() => CmbClientQuantity.Text)))
                    break;
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
        }

        private void Connection()
        {
            while (true)
            {
                Hello();
            }
        }

        private bool Hello()
        {
            var ipfromFile = File.ReadAllLines(@"config.cfg");
            IPAddress.TryParse(ipfromFile[0], out IPAddress ipServer);
            IPAddress.TryParse(ipfromFile[1], out IPAddress ipBackup);

            var localep = new IPEndPoint(ipServer, 0);
            TcpClient client = new TcpClient(localep);

            var remotep = new IPEndPoint(ipBackup, 6666);
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
                                if (response != "Got it")
                                {
                                    throw new Exception();
                                }
                            }
                            Thread.Sleep(2000);
                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show("Der BackupServer ist nicht mehr erreichbar!");
                    client.Close();
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Kein BackupServer zur Verfügung!");
                client.Close();
                return false;
            }
        }


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

        private void ImageFraktal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPos = e.GetPosition(imageFraktal);

            

            selection.Width = 0;
            selection.Height = 0;
            selection.Visibility = Visibility.Visible;
        }

        private void ImageFraktal_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton==MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(imageFraktal);
                Vector diff = mousePos - mouseDownPos;

                Point topLeft = mouseDownPos;

                if (diff.X < 0)
                {
                    topLeft.X = mousePos.X;
                    diff.X = -diff.X;
                }
                if (diff.Y < 0)
                {
                    topLeft.Y = mousePos.X;
                    diff.Y = -diff.Y;
                }

                selection.Width = diff.X;
                selection.Height = diff.Y;


            }
        }

        private void ImageFraktal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        
        private void ImageFraktal_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform)((TransformGroup)imageFraktal.RenderTransform).Children.First(tr =>
              tr is ScaleTransform);
            var zoom = e.Delta > 0 ? .2 : -.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }
        /*
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
        */
        }
}