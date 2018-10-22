using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().Wait();
        }

        static async Task AsyncMain()
        {
            TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Loopback, 0));
            await client.ConnectAsync(IPAddress.Loopback, 5566);

            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    using (var reader = new StreamReader(stream, Encoding.ASCII, true, 4096, leaveOpen: true))
                    {
                        int threadid = Thread.CurrentThread.ManagedThreadId;
                        string answer = reader.ReadLine();
                        Console.WriteLine(answer);
                        Console.WriteLine($"Current thread {threadid}: Calculating...");
                        Thread.Sleep(2000);
                        
                        Console.WriteLine($"Current thread {threadid}: Finished");
                    }

                    using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, leaveOpen: true))
                    {
                        string msg = "Client 1 finished calculation!";
                        writer.WriteLine(msg);
                    }
                }
            }
        }
    }
}