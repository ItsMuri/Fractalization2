﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private TcpListener listener;
        public MainWindow()
        {
            InitializeComponent();
            var localep = new IPEndPoint(IPAddress.Any, 3333);
            listener = new TcpListener(localep);
            listener.Start();

            Task T = new Task(() =>
            {
                //Hello();
                Connection();
            });
            T.Start();
        }

        private void ZeichneFraktal()
        {

            int cntInterations = Convert.ToInt32(Dispatcher.Invoke(() => tbIterations.Text));
            FraktalTask ft = new FraktalTask();
            // in ft werden die verschiedenen 
            //Versuche das Fraktal in 2 Sektoren zu unterteilen!!!
            Bitmap bm = new Bitmap(Convert.ToInt32(imageFraktal.Width), Convert.ToInt32(imageFraktal.Height));
            Bitmap lowerBm = new Bitmap(Convert.ToInt32(imageFraktal.Width / 2), Convert.ToInt32(imageFraktal.Height / 2));
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

        private void Senden(PropsOfFractal myFraktal)
        {
            //Hier werden nun die Informationen gesendet
            using (TcpClient client = listener.AcceptTcpClient())
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

                using (NetworkStream stream = client.GetStream())
                {
                    CryptoStream encryptStream = new CryptoStream(stream, cryptic.CreateEncryptor(), CryptoStreamMode.Write);

                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    serializer.WriteObject(encryptStream, myFraktal);
                   
                    encryptStream.FlushFinalBlock();
                    
                    client.Client.Shutdown(SocketShutdown.Send);


                 
                    CryptoStream decryptStream = new CryptoStream(stream, cryptic.CreateDecryptor(), CryptoStreamMode.Read);
                    var verabeiteteDaten = (Bitmap)System.Drawing.Image.FromStream(decryptStream);
                    
                    //var BitmSerializer = new DataContractSerializer(typeof(Bitmap));
                   // Bitmap verabeiteteDaten = (Bitmap)BitmSerializer.ReadObject(decryptStream);
                    //Bitmap verabeiteteDaten = (Bitmap)BitmSerializer.ReadObject(stream);
                    FraktalAnzeigen(verabeiteteDaten);

                    decryptStream.Close();

                    stream.Close();
                };
            };
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

            Task t2 = new Task(() =>
                {
                    Senden(myFraktal);
                });
            t2.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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