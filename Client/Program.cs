using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Awaiting Connection");
            AsyncMain().Wait();

            Console.ReadKey();
        }

        static async Task AsyncMain()
        {
            TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Loopback, 0));
            await client.ConnectAsync(IPAddress.Loopback, 5566);

            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    var serializer = new DataContractSerializer(typeof(FraktalClnt));
                    FraktalClnt fobj = (FraktalClnt)serializer.ReadObject(stream);

                    Calculate(fobj, stream);
                }
            }
        }

        private static void Calculate(FraktalClnt fobj, NetworkStream stream)
        {
            for (int x = 0; x < fobj.KoordinatenX; x++)
            {
                for (int y = 0; y < fobj.KoordinatenY; y++)
                {
                    double a = (double)(x - fobj.KoordinatenX / 2) / (double)(fobj.KoordinatenX / 4);
                    double b = (double)(y - fobj.KoordinatenY / 2) / (double)(fobj.KoordinatenY / 4);
                    ComplexClnt c = new ComplexClnt(a, b);
                    ComplexClnt z = new ComplexClnt(0, 0);
                    int it = 0;
                    double[] coordinates =
                    {
                        a, b
                    };

                    do
                    {
                        it++;
                        z.Square();
                        z.Add(c);

                        if (z.Magnitude() > 2.0) break;

                        coordinates[0] = a;
                        coordinates[1] = b;
                        var ser = new DataContractSerializer(typeof(double));
                        ser.WriteObject(stream, coordinates);

                    } while (it <= fobj.Iteration);
                }
            }
        }
    }
}