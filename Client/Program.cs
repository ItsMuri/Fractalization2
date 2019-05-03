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
using System.Windows.Media.Imaging;
using FractalLibrary;

//using Server;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();

            ServerConnenction();
        }

        public static void ServerConnenction()
        {
            var ipfromFile = File.ReadAllLines(@"config.cfg");

            IPAddress.TryParse(ipfromFile[0], out IPAddress ipServer);
            IPAddress.TryParse(ipfromFile[1], out IPAddress ipBackup);

            while (true)
            {
                bool success = ProcessRequests(ipServer, 3333);

                if (!success)
                {
                    Console.WriteLine("Wechsle zu Backupserver");
                    ProcessRequests(ipBackup, 2222);
                }
                else
                {
                    Console.WriteLine("Wechsle zu Server");
                    ProcessRequests(ipServer, 3333);
                }
                Thread.Sleep(2000);
            }
        }

        private static bool ProcessRequests(IPAddress ipAddress, int port)
        {
            var localep = new IPEndPoint(IPAddress.Any, 0);

            TcpClient client = new TcpClient(localep);

            var ipfromFile = File.ReadAllLines(@"config.cfg");
            IPAddress.TryParse(ipfromFile[0], out IPAddress ipServer);

            var remotep = new IPEndPoint(ipServer, port);
            try
            {
                client.Connect(remotep);

                string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                string port_client = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();

                if (remotep.Port == 3333)
                {
                    Console.WriteLine($"Verbunden mit Server {ip}, {port_client}");
                }
                else if (remotep.Port == 2222)
                {
                    Console.WriteLine($"Verbunden mit Backup-Server {ip}, {port_client}");
                }

                AesCryptoServiceProvider cryptic = new AesCryptoServiceProvider();

                string key = "Tg6VU5ZzKjNrR0UoYrVVgPdZafNtLT4XSwyWQRvna1w=";
                string IV = "NjjwRHuRPEgLAT0qD+0UaQ==";

                byte[] keybyte = Convert.FromBase64String(key);
                byte[] ivbyte = Convert.FromBase64String(IV);

                cryptic.Key = keybyte;
                cryptic.IV = ivbyte;

                //cryptic.GenerateKey();
                //cryptic.GenerateIV();

                using (NetworkStream stream = client.GetStream())
                {
                    CryptoStream decryptStream = new CryptoStream(stream, cryptic.CreateDecryptor(), CryptoStreamMode.Read);

                    Console.WriteLine("Iterating ...");
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    PropsOfFractal fobj = (PropsOfFractal)serializer.ReadObject(decryptStream);

                    int stripe = (int)fobj.ImgWidth / fobj.ClientCount;
                    Bitmap bm = new Bitmap(stripe, Convert.ToInt32(fobj.ImgHeight));
                    Calculate(fobj, stripe, ref bm);

                    CryptoStream encryptStream = new CryptoStream(stream, cryptic.CreateEncryptor(), CryptoStreamMode.Write);

                    bm.Save(encryptStream, ImageFormat.Bmp);

                    encryptStream.FlushFinalBlock();
                    encryptStream.Close();

                    decryptStream.Close();

                    //client.Client.Shutdown(SocketShutdown.Send);
                }
                client.Close();
                return true;
            }
            catch (Exception e2)
            {
                Console.WriteLine("Exception caught ..." + e2.Message);
                return false;
            }
        }
        private static void Calculate(PropsOfFractal fobj, int stripe, ref Bitmap bm)
        {
            Console.WriteLine("ID: " + fobj.Id);
            int myStripeBegin = stripe * fobj.Id;

            // x and y are the coordinates in the bitmap image that represents a stripe of the full image
            // (myStripeBegin + x) is the correct 'x' value for computing the fractal
            for (int x = 0; x < stripe; x++)
            {
                for (int y = 0; y < fobj.ImgHeight; y++)
                {
                    double a = (double)((myStripeBegin + x) - fobj.ImgWidth / 2) / (double)(fobj.ImgWidth / 4);
                    double b = (double)(y - fobj.ImgHeight / 2) / (double)(fobj.ImgHeight / 4);
                    ComplexClnt c = new ComplexClnt(a, b);
                    ComplexClnt z = new ComplexClnt(0, 0);
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
                    } while (it <= fobj.IterationsCount);
                    //bm.SetPixel(x, y, it < fobj.IterationsCount ? Color.Red : Color.Blue);



                    var faktor = 255 / (it * 2);

                    bm.SetPixel(x, y, Color.FromArgb(0, 60 + faktor, 0 + faktor, 70 + faktor));
                }
            }
        }

    }
}