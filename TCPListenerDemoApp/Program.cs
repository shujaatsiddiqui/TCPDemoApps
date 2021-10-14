using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TCPListenerDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReadFile();
            //IPAddress localhost = IPAddress.Parse("10.19.20.57");
            //TcpListener tcpListener = new TcpListener(IPAddress.Any, 58765);

            var socketAccept = new TcpListener(IPAddress.Any, 58765);
            // 2min
            //socketAccept.Server.SendTimeout = Convert.ToInt32(120000);
            //socketAccept.Server.ReceiveTimeout = Convert.ToInt32(120000);
            //socketAccept.Server.ReceiveBufferSize = Convert.ToInt32(120000);
            socketAccept.Start(5);

            socketAccept.BeginAcceptTcpClient(OnAccept, socketAccept);
            Console.ReadLine();

        }

        private static void OnAccept(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;


            // End the operation and display the received data on
            // the console.
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);

            // listener.AcceptTcpClient();

            var gatewayConnectionPlugin = new EthernetGatewayHandler();
            gatewayConnectionPlugin.Initialize(tcpClient);
            gatewayConnectionPlugin.InitializeAsync();

            new Thread(() => Send(gatewayConnectionPlugin)).Start();

            // ReadData(tcpClient);



            //TestClass cc = new TestClass();
            //cc.SocketClassObj = tcpClient.Client;
            //cc.SocketClassObj.Send(new byte[] { 01, 00, 00, 03, 02, 08, 02, 172 });
            //Console.WriteLine("Main done...");
            //Console.ReadKey();

            listener.BeginAcceptTcpClient(OnAccept, listener);
        }

        private static void Send(EthernetGatewayHandler gatewayConnectionPlugin)
        {
            int count = 1;
            while (true)
            {
                byte[] messageBytesToSend = Encoding.UTF8.GetBytes("This is a very serious message from the server over TCP." + count);
                gatewayConnectionPlugin.Send(messageBytesToSend);
                Thread.Sleep(5000);
                count++;
            }
        }

        private static void ReadData(TcpClient tcpClient)
        {
            StringBuilder messageBuilder = new StringBuilder();

            int bytesRead;
            using (NetworkStream ns = tcpClient.GetStream())
            {
                int messageChunkSize = 10;
                do
                {
                    byte[] chunks = new byte[messageChunkSize];
                    bytesRead = ns.Read(chunks, 0, chunks.Length);
                    messageBuilder.Append(Encoding.UTF8.GetString(chunks));
                }
                while (bytesRead != 0);
            }
            Console.WriteLine(messageBuilder.ToString());
        }

        private static void OnAcceptSameClient(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;


            // End the operation and display the received data on
            // the console.
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);

            // listener.AcceptTcpClient();

            //var gatewayConnectionPlugin = new EthernetGatewayHandler();
            //gatewayConnectionPlugin.Initialize(tcpClient);


            StringBuilder messageBuilder = new StringBuilder();
            while (true)
            {
                using (tcpClient)
                {
                    int bytesRead;
                    NetworkStream ns = tcpClient.GetStream();
                    int messageChunkSize = 10;
                    do
                    {
                        byte[] chunks = new byte[1024];
                        bytesRead = ns.Read(chunks, 0, chunks.Length);
                        messageBuilder.Append(Encoding.UTF8.GetString(chunks));
                    }
                    while (bytesRead != 0);
                    Console.WriteLine(messageBuilder.ToString());
                }
            }

            //TestClass cc = new TestClass();
            //cc.SocketClassObj = tcpClient.Client;
            //cc.SocketClassObj.Send(new byte[] { 01, 00, 00, 03, 02, 08, 02, 172 });
            //Console.WriteLine("Main done...");
            //Console.ReadKey();

            //listener.BeginAcceptTcpClient(OnAccept, listener);
        }
        private static int ReadData(TcpClient tcpClient, StringBuilder messageBuilder)
        {
            while (true)
            {
                int bytesRead;
                using (NetworkStream ns = tcpClient.GetStream())
                {
                    int messageChunkSize = 10;
                    do
                    {
                        byte[] chunks = new byte[messageChunkSize];
                        bytesRead = ns.Read(chunks, 0, chunks.Length);
                        messageBuilder.Append(Encoding.UTF8.GetString(chunks));
                    }
                    while (bytesRead != 0);
                }
                Console.WriteLine(messageBuilder.ToString());
                return bytesRead;
            }
        }

        static void ReadFile()
        {
            var devicePolicyFiles = Directory.GetFiles(@"C:\LnT UBIQSens\PolicyFiles\DevicePolicy");
            foreach (var devicePolicyFile in devicePolicyFiles)
            {
                var str = File.ReadAllText(devicePolicyFile);
                var doc = new XmlDocument();
                doc.LoadXml(str);
                var nodeList = doc.DocumentElement.SelectSingleNode("/Device/MacId");
                if (nodeList.InnerText.Equals("B482550093EC", StringComparison.InvariantCultureIgnoreCase))
                {

                }
            }
        }
    }

    public class TestClass
    {
        public Socket SocketClassObj { get; set; }
    }
}
