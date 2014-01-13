using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketAsyncClientTest
{
    public class AsyncClient
    {
        public static string theResponse = "";
        public static byte[] buffer = new byte[1024];
        public static ManualResetEvent ConnectDone = new ManualResetEvent(false);
        public static ManualResetEvent SendDone = new ManualResetEvent(false);
        public static ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            try
            {
                Thread thr = Thread.CurrentThread;
                Console.WriteLine("Main Thread State:" + thr.ThreadState);

                IPHostEntry iphost = Dns.GetHostEntry("127.0.0.1");
                IPAddress ipaddr = iphost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipaddr, 11000);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sender.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallback), sender);
                ConnectDone.WaitOne();

                string data = "This is a Test.<TheEnd>";
                //for (int i = 0; i < 72; i++)
                //    data += i.ToString() + ":" + (new string('=', i));
                buffer = Encoding.ASCII.GetBytes(data);

                sender.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), sender);
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine(i);
                    Thread.Sleep(10);
                }


                sender.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), sender);

                Console.WriteLine("Response received:{0}", theResponse);

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void ConnectCallback(IAsyncResult ar)
        {
            Thread thr = Thread.CurrentThread;
            Console.WriteLine("ConnectCallback Thread State:" + thr.ThreadState);
            Socket sClient = (Socket)ar.AsyncState;
            sClient.EndConnect(ar);
            Console.WriteLine("Socket connected to {0}", sClient.RemoteEndPoint.ToString());
            ConnectDone.Set();
        }

        public static void SendCallback(IAsyncResult ar)
        {
            Thread thr = Thread.CurrentThread;
            Console.WriteLine("SendCallback Thread State:" + thr.ThreadState);
            Socket sClient = (Socket)ar.AsyncState;
            int bytesSent = sClient.EndSend(ar);
            Console.WriteLine("Send {0} bytes to server.", bytesSent);

        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            Thread thr = Thread.CurrentThread;
            Console.WriteLine("ReceiveCallback Thread State:" + thr.ThreadState);
            Socket sClient = (Socket)ar.AsyncState;
            int bytesRead = sClient.EndReceive(ar);
            if (bytesRead > 1)
            {
                theResponse += Encoding.ASCII.GetString(buffer, 0, bytesRead);
                sClient.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), sClient);
            }
        }
    }
}
