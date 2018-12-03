using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            Task T = new Task(() =>
            {
                Hello();
            });
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
        }
        
        private  void Hello()
        {
           // Console.WriteLine("---Server---");
            var localep = new IPEndPoint(IPAddress.Loopback, 5555);
            TcpClient client = new TcpClient(localep);

            var remotep = new IPEndPoint(IPAddress.Loopback, 6666);
            try
            {
                client.Connect(remotep);
                string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                string port = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
               // Console.WriteLine($"Server hat sich mit Backupserver {ip}, {port} verbunden");

                try
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        int itanzahl = Convert.ToInt32(tbIterations.Text);
                        using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                        {
                            writer.Write(itanzahl);
                        };

                        while (true)
                        {
                            using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                            {
                                writer.WriteLine("Hello");
                            }
                            using (var reader = new StreamReader(stream, Encoding.ASCII, true, 4096, leaveOpen: true))
                            {
                                string response = reader.ReadLine();
                                //Console.WriteLine(response);
                            }
                            System.Threading.Thread.Sleep(2000);
                        }

                    }
                }
                catch (Exception)
                {
                    //  Console.WriteLine("Backupserver ist down!");

                    MessageBox.Show("Der BackupServer ist down!");
                    Console.ReadKey();
                }
                client.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Kein BackupServer zur Verfügung!");
               // Console.WriteLine("Kein Backupserver zur Verfügung");
            }
            //////catch (Exception)
            //////{
            //////    //code der ausgeführt wird wenn der server keine hello pakete mehr schickt
            //////    int iterationsCount = Convert.ToInt32(tbIterations.Text);
            //////    PropsOfFractal myFraktal = new PropsOfFractal(iterationsCount);
            //////    myFraktal.imgWidth = imageFraktal.Width;
            //////    myFraktal.imgHeight = imageFraktal.Height;


            //////    Console.WriteLine("Server ist down");
            //////    sw.Stop();
            //////    TimeSpan ts = sw.Elapsed;
            //////    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
            //////    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            //////    sw.Reset();
            //////    Console.WriteLine("Runtime " + elapsedTime);
            //////    Console.WriteLine("Übernehmen der Rolle des Hauptservers");
            //////    Senden(myFraktal);
            //////}

        }


        private void FraktalAnzeigen(Bitmap verabeiteteDaten)
        {
            //BitmapImage bmi = BitmapToImageSource(verabeiteteDaten);
            imageFraktal.Dispatcher.Invoke(() => imageFraktal.Source = BitmapToImageSource(verabeiteteDaten));
            MessageBox.Show("Fertig");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int iterationsCount = Convert.ToInt32(tbIterations.Text);
            PropsOfFractal myFraktal = new PropsOfFractal(iterationsCount);
            myFraktal.imgWidth = imageFraktal.Width;
            myFraktal.imgHeight = imageFraktal.Height;
          

            Task t1 = new Task(() =>
            {
                Hello();
            });
            t1.Start();

            Task t2 = new Task(() =>
                {
                    Senden(myFraktal);
                });
            t2.Start();
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