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

        public void SendMessageFromSocket(int port, int counterCycle)
        {

            //IPHostEntry ipHost = Dns.GetHostEntry("localhost");
           
            //IPAddress ipAddr = ipHost.AddressList[0];
           // IPEndPoint ipPoint = new IPEndPoint(ipAddr, 30000);

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("192.168.0.1"), port);
            //IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("10.245.8.158"), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           // Socket socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CreateXmlDocument createXML = new CreateXmlDocument();
            ScanerLsd scaner = new ScanerLsd();
            try
            {
                socket.Bind(ipPoint);
                socket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting connect {0}", ipPoint + "\n");
                    counterCycle += 1;

                    Socket handler = socket.Accept();// Программа приостанавливается, ожидая входящее соединение
                    try
                    {                        
                        scaner.ConnectLsd();
                        string data = null;
                        string res = null;
                        // Мы дождались клиента, пытающегося с нами соединиться
                        byte[] bytes = new byte[1024];//256
                        int bytesRec = handler.Receive(bytes);
                        if (bytesRec > 0)
                        {
                            Console.Write("bytesRec: " + bytesRec + "\n");
                            data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                            Console.Write("Received from client: " + data + "\n");
                            // if (data.Contains("<scanLSD id=\"1\"></scanLSD>"))
                            if (data.Contains("1"))
                            {
                                scaner.StartScan();
                                Console.Write("Cycle is: " + counterCycle + "\n\n");
                                 byte[] msg1 = Encoding.UTF8.GetBytes(createXML.CreateXmlDoc(scaner.DotPoint.X, scaner.DotPoint.Y, scaner.angl_v1).ToString());
                                //[] msg1 = Encoding.UTF8.GetBytes(createXML                                        .CreateXmlDoc(100, 200, 90).ToString());

                                res += Encoding.UTF8.GetString(msg1, 0, msg1.Length);
                                if (msg1.Length > 0)
                                {
                                    handler.Send(msg1);
                                    Console.Write("Responce to client: " + res + "\n\n");
                                    Console.WriteLine("Connection is completed.");
                                }
                                else
                                {
                                    Console.Write("Responce to client: " + msg1.ToString() + "\n\n");
                                }
                            }
                            if (data.IndexOf("<TheEnd>") > -1)
                            {
                                Console.WriteLine("Сервер завершил соединение с клиентом.");
                                break;
                            }
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                        else
                        {
                            Console.WriteLine("пустое сообщение от клиента.");

                        }

                    }


                    catch (Exception e)
                    {

                        Console.WriteLine(e.ToString());                       
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());


            }
            finally
            {
                Console.WriteLine("Finally ");
                socket.Dispose();
                socket.Close(); ;

            }
        }
    }
}
