using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpClientDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectTCP();

            // ConnectTCPSameClient();
            //for (int i = 0; i <= 6 ; i++)
            //{
            //    // new Thread(() => ConnectTCP()).Start();
            //    ConnectTCP(i.ToString());
            //    //Thread.Sleep(00); // wait 2 min send new request
            //}

            //Console.ReadLine();
            //////communication using tcp tunneling.
            ////List<int> count = Enumerable.Range(1, 10).ToList();
            ////Parallel.ForEach(count, i =>
            //// {


            //// });

            //Console.WriteLine("Main done...");
            //Console.ReadLine();

            //  ConnectTCP();
        }


        static void ConnectTCP()
        {
            try
            {
                // TcpClient tcpClient = new TcpClient("54.190.26.32", 58765);
                TcpClient tcpClient = new TcpClient("localhost", 58765);
                NetworkStream ns = tcpClient.GetStream();

                new Thread(() => Send(ns)).Start();
                new Thread(() => Read(ns)).Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Read(NetworkStream ns)
        {
            while (true)
            {
                var data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = ns.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine(responseData);
            }
        }

        private static void Send(NetworkStream ns)
        {
            int count = 1;
            while (true)
            {
                byte[] messageBytesToSend = Encoding.UTF8.GetBytes("This is a very serious message from the client over TCP." + count);
                ns.Write(messageBytesToSend, 0, messageBytesToSend.Length);
                Thread.Sleep(5000);
                count++;
            }

        }

        static void ConnectTCP(string message)
        {
            try
            {
                // TcpClient tcpClient = new TcpClient("54.190.26.32", 58765);
                TcpClient tcpClient = new TcpClient("localhost", 58765);
                using (NetworkStream ns = tcpClient.GetStream())
                {
                    using (BufferedStream bs = new BufferedStream(ns))
                    {
                        byte[] messageBytesToSend = Encoding.UTF8.GetBytes("This is a very serious message from the client over TCP." + message);
                        bs.Write(messageBytesToSend, 0, messageBytesToSend.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ConnectTCPSameClient()
        {
            try
            {
                TcpClient tcpClient = new TcpClient("localhost", 58765);

                for (int i = 0; i < 10; i++)
                {
                    NetworkStream ns = tcpClient.GetStream();
                    BufferedStream bs = new BufferedStream(ns);
                    byte[] messageBytesToSend = Encoding.UTF8.GetBytes("This is a very serious message from the client over TCP." + i);
                    bs.Write(messageBytesToSend, 0, messageBytesToSend.Length);
                    bs.Flush();
                    //bs.Dispose();
                    //using ()
                    //{
                    //    using ()
                    //    {

                    //    }
                    //}
                    // tcpClient.Connect("localhost", 58765);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
