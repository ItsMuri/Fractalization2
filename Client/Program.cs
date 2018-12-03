using System;
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
using System.Windows.Media.Imaging;
using SerializedFraktal;
//using Server;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Awaiting Connection");
            
            ServerConnenction();

            Console.ReadKey();
        }

        

        public static void ServerConnenction()
        {
            Console.WriteLine("---Client1---");
            var localep = new IPEndPoint(IPAddress.Loopback, 0);
            TcpClient client = new TcpClient(localep);

            var remotep = new IPEndPoint(IPAddress.Loopback, 3333);
            try
            {
                client.Connect(remotep);
                string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                string port = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                Console.WriteLine($"Verbunden mit Hauptserver {ip}, {port}");
                using (NetworkStream stream = client.GetStream())
                {
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    PropsOfFractal fobj = (PropsOfFractal)serializer.ReadObject(stream);

                    Bitmap bm = new Bitmap(400, 400);
                    Calculate(fobj, ref bm);

                    var ser = new DataContractSerializer(typeof(Bitmap));
                    ser.WriteObject(stream, bm);
                
                }
            }
            catch (Exception)
            {

                Console.WriteLine("Wechsle zu Backupserver");
                var bremoteep = new IPEndPoint(IPAddress.Loopback, 2222);
                TcpClient client2 = new TcpClient(localep);

                try
                {
                    client2.Connect(bremoteep);
                    string ip = ((IPEndPoint)client2.Client.RemoteEndPoint).Address.ToString();
                    string port = ((IPEndPoint)client2.Client.RemoteEndPoint).Port.ToString();
                    Console.WriteLine($"Verbunden mit BackupServer {ip}, {port}");

                    using (NetworkStream stream = client2.GetStream())
                    {
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII, 2048, true))
                        {
                            var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                            PropsOfFractal fobj = (PropsOfFractal)serializer.ReadObject(stream);

                            Bitmap bm = new Bitmap(400, 400);
                            Calculate(fobj, ref bm);

                            var ser = new DataContractSerializer(typeof(Bitmap));
                            ser.WriteObject(stream, bm);
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            Console.ReadKey();
        }

        private static void Calculate(PropsOfFractal fobj, ref Bitmap bm)
        {
            for (int x = 0; x < fobj.imgWidth; x++)
            {
                for (int y = 0; y < fobj.imgHeight; y++)
                {
                    double a = (double) (x - fobj.imgWidth / 2) / (double) (fobj.imgWidth / 4);
                    double b = (double) (y - fobj.imgHeight / 2) / (double) (fobj.imgHeight / 4);
                    ComplexClnt c = new ComplexClnt(a, b);
                    ComplexClnt z = new ComplexClnt(0, 0);
                    int it = 0;
                    //double[] coordinates =
                    //{
                    //    a, b
                    //};

                    do
                    {
                        it++;
                        z.Square();
                        z.Add(c);

                        if (z.Magnitude() > 2.0) { break;}

                        //coordinates[0] = a;
                        //coordinates[1] = b;
                    } while (it <= fobj.IterationsCount);
                    //Console.WriteLine($"{x}:{y}:{it}");
                    bm.SetPixel(x, y, it < fobj.IterationsCount ? Color.Red : Color.Blue);
                }
            }
        }
    }
}