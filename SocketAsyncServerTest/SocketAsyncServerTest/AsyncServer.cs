using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace SocketAsyncServerTest
{
    class AsyncServer
    {
        public static byte[] buffer=new byte[1024];

        public static ManualResetEvent scoketEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("Main Thread ID:"+AppDomain.GetCurrentThreadId());

            byte[] bytes=new byte[1024];

            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
            Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sListener.Bind(ipEndPoint);
            sListener.Listen(10);
            
            while(true)
            {
                Console.WriteLine("Waiting for a connection...");

                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);

                sListener.BeginAccept(aCallback, sListener);

                scoketEvent.WaitOne();
            }
        }

        public static void AcceptCallback(IAsyncResult ar) 
        {
            Console.WriteLine("AcceptCallback Thread ID:"+AppDomain.GetCurrentThreadId());

            Socket listener = (Socket)ar.AsyncState;
            Socket handle = listener.EndAccept(ar);

            handle.BeginReceive(buffer,0,buffer.Length,0,new AsyncCallback(ReceiveCallback),handle);
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("ReceiveCallback Threa d ID :"+AppDomain.GetCurrentThreadId());

            string content =string.Empty;
            Socket handler=(Socket )ar.AsyncState;
            int bytesRead=handler.EndReceive(ar);

            if(bytesRead>0)
            {
                content+=Encoding .ASCII.GetString(buffer,0,bytesRead);

                if(content.IndexOf("<TheEnd>")>1)
                {
                    Console.WriteLine("Read{0}bytes from socket .\n Data:{1}",content.Length,content);
                    byte[] byteData=Encoding.ASCII.GetBytes(content);

                    handler.BeginSend(byteData,0,byteData.Length,0,new AsyncCallback(SendCallback),handler);
                }
                else
                {
                    handler.BeginReceive(buffer,0,buffer.Length,0,new AsyncCallback(ReceiveCallback),handler);

                }
            }
        }

            public static void SendCallback(IAsyncResult ar)
            {
                Console.WriteLine("SendCallback Thread ID:"+AppDomain.GetCurrentThreadId());

                Socket handler =(Socket)ar.AsyncState;
                int bytesSent =handler.EndSend(ar);

                Console.WriteLine("Sent{0}bytes to Client.",bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                scoketEvent.Set();
            }
        }
    
}
