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
using FractalLibrary;

//using Server;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.ReadKey();
                TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
                client.Connect(IPAddress.Loopback, 2222);
                Console.WriteLine("Connected");

                using (NetworkStream stream = client.GetStream())
                {
                    var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                    PropsOfFractal fobj = (PropsOfFractal)serializer.ReadObject(stream);

                    int stripe = (int)fobj.ImgWidth / fobj.ClientCount;
                    Bitmap bm = new Bitmap(stripe, Convert.ToInt32(fobj.ImgHeight));
                    Calculate(fobj, stripe, ref bm);

                    var ser = new DataContractSerializer(typeof(Bitmap));
                    ser.WriteObject(stream, bm);
                }

                client.Close();
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
                    bm.SetPixel(x, y, it < fobj.IterationsCount ? Color.Red : Color.Blue);
                }
            }



            /*
            if (fobj.Id == 1)
            {
                for (int x = 0; x < fobj.imgWidth/2; x++)
                {
                    for (int y = 0; y < fobj.imgHeight; y++)
                    {
                        double a = (double) (x - fobj.imgWidth / 2) / (double) (fobj.imgWidth / 4);
                        double b = (double) (y - fobj.imgHeight / 2) / (double) (fobj.imgHeight / 4);
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
                        bm.SetPixel(x, y, it < fobj.IterationsCount ? Color.Red : Color.Blue);
                    }
                }
            }
            else if (fobj.Id == 2)
            {
                for (double x = fobj.imgWidth/2; x < fobj.imgWidth; x++)
                {
                    for (int y = 0; y < fobj.imgHeight; y++)
                    {
                        double a = (double)(x - fobj.imgWidth / 2) / (double)(fobj.imgWidth / 4);
                        double b = (double)(y - fobj.imgHeight / 2) / (double)(fobj.imgHeight / 4);
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
                        bm.SetPixel(Convert.ToInt32(x), y, it < fobj.IterationsCount ? Color.Red : Color.Blue);
                    }
                }
            }
            */
        }
    }
}