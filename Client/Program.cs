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


namespace Client
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("---Client1---");

            Console.WriteLine("Awaiting Connection");

            Thread.Sleep(10000);
            ServerConnenction();
            //TryToConnectServer();


            //Console.ReadLine();
            //Console.ReadKey();
        }
        // Es wird alle 10 Sekunden versucht sich mit dem Hauptserver zu verbinden. Der Client versucht sich zu verbinden. Wenn die Verbindung erfolgreich war
        // wird die Methode Server Connection erneut aufgerufen.
        //public static bool TryToConnectServer()
        //{
        //    var localep = new IPEndPoint(IPAddress.Loopback, 0);
        //    TcpClient client = new TcpClient(localep);
        //    var remoteep = new IPEndPoint(IPAddress.Loopback, 3333);

        //    try
        //    {
        //        Console.WriteLine("Versuche mit dem Haupt Server zu verbinden");
        //        client.Connect(remoteep);

        //        string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        //        string port_client = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
        //        Console.WriteLine($"Erneut verbunden mit Haupt Server {ip}, {port_client}");

        //        client.Close();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }

        //}
        //Derzeitiges Problem: Hauptserver +  client: Hauptserver sagt berechne, client schickt berechnung zurück und Server zeigt an.
        //Dann, obwohl der Hauptserver noch läuft, verbindet sich der Client mit dem Backup Server und wartet dort auf die neue Eingabe
        //und berechnet sie dann an den Backup

        public static void ServerConnenction()
        {
            // Forschleife anzahl versuche / min überprüfen muss noch gemacht werden
            while (true)
            {
                bool success = ProcessRequests(3333);

                if (!success)
                {
                    Console.WriteLine("Wechsle zu Backupserver");
                    ProcessRequests(2222);

                    //Thread.Sleep(10000);
                    //for (int i = 0; i <= 60; i++)
                    //{
                    //    bool ConnectionWorks = TryToConnectServer();
                    //    if (ConnectionWorks == true)
                    //    {
                    //        ProcessRequests(3333);
                    //        Console.WriteLine("Es hat geklappt");
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("Leider kein bs verfügbar!");
                    //        i++;
                    //        Thread.Sleep(2000);
                    //    }
                    //}

                }
                else
                {
                    Console.WriteLine("Wechsle zu Server");
                    ProcessRequests(3333);
                }
                Thread.Sleep(1000);
            }
        }


        private static bool ProcessRequests(int port)
        {
            var localep = new IPEndPoint(IPAddress.Loopback, 0);

            //while (true)
            //{
                TcpClient client = new TcpClient(localep);

                var remotep = new IPEndPoint(IPAddress.Loopback, port);
                try
                {
                    client.Connect(remotep);
                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    string port_client = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                    if (port == 3333)
                    {
                        Console.WriteLine($"Verbunden mit Server {ip}, {port_client}");
                    }
                    else if (port == 2222)
                    {
                        Console.WriteLine($"Verbunden mit Backup Server {ip}, {port_client}");
                    }
                    else
                    {
                        Console.WriteLine($"Verbunden mit Unbekanntem Server {ip}, {port_client}");
                    }

                    using (NetworkStream stream = client.GetStream())
                    {
                        Console.WriteLine("Iterating ...");
                        var serializer = new DataContractSerializer(typeof(PropsOfFractal));
                        PropsOfFractal fobj = (PropsOfFractal)serializer.ReadObject(stream);

                        Bitmap bm = new Bitmap(400, 400);
                        Calculate(fobj, ref bm);

                        var ser = new DataContractSerializer(typeof(Bitmap));
                        ser.WriteObject(stream, bm);
                    }
                    client.Close();
                return true;
                }
                catch (Exception e2)
                {
                    Console.WriteLine("Exception caught ..." + e2.Message);
                    return false;
                }
            //}
        }

        private static void Calculate(PropsOfFractal fobj, ref Bitmap bm)
        {
            for (int x = 0; x < fobj.imgWidth; x++)
            {
                for (int y = 0; y < fobj.imgHeight; y++)
                {
                    double a = (double)(x - fobj.imgWidth / 2) / (double)(fobj.imgWidth / 4);
                    double b = (double)(y - fobj.imgHeight / 2) / (double)(fobj.imgHeight / 4);
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

                        if (z.Magnitude() > 2.0) { break; }

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