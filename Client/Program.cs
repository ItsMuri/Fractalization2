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
            //AsyncMain().Start();
            //AsyncMain().Wait();
            AsyncMain();

            Console.ReadKey();
        }

        //static async Task AsyncMain()
        private static void AsyncMain()
        {
            TcpListener myListener = new TcpListener(IPAddress.Loopback, 5566);
            myListener.Start();
            while (true)
            {
                TcpClient client = myListener.AcceptTcpClient();


                using (NetworkStream stream = client.GetStream())
                {
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    PropsOfFractal fobj = (PropsOfFractal) serializer.ReadObject(stream);

                    Bitmap bm = new Bitmap(400, 400);
                    Calculate(fobj, ref bm);

                    var ser = new DataContractSerializer(typeof(Bitmap));
                    ser.WriteObject(stream, bm);
                }

                client.Close();
            }
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