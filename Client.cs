using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LSD_CLIENT_AQUA
{
    internal class Client
    {

        public void SendMessageFromSocket( int port)
        {


            IPAddress ipAddr = IPAddress.Parse("10.245.8.151");
            //IPAddress ipAddr = IPAddress.Parse("10.244.1.173");

            IPEndPoint ipPoint = new IPEndPoint(ipAddr, 49153);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CreateXmlDocument createXML = new CreateXmlDocument();
            ScanerLsd scaner = new ScanerLsd();
            

            try
            {
                socket.Bind(ipPoint);
                socket.Listen(10);
                while (true)
                {
                    scaner.ConnectLsd();
                    scaner.StartScan();
                    byte[] msg1 = Encoding.UTF8.GetBytes(createXML.CreateXmlDoc(scaner.DotPoint.X, scaner.DotPoint.Y, scaner.angl_v1).ToString());
                    Console.WriteLine("Waiting connect {0}", ipPoint);
                    Socket handler = socket.Accept();
                    string data = null;
                    string res = null;
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                   Console.Write("Received from client: " + data + "\n\n");
                    if (data != null)
                    {

                    }
                   
                    res += Encoding.UTF8.GetString(msg1, 0, msg1.Length);

                    handler.Send(msg1);
                    Console.Write("Responce to client: " + res + "\n\n");

                    if (data.IndexOf("<TheEnd>", StringComparison.Ordinal) > -1)
                    {
                        SendMessageFromSocket(port);
                        Console.WriteLine("Connection is completed.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }




        }
    }

}
