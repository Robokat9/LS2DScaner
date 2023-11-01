using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
namespace LSD_CLIENT_AQUA
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            CreateXmlDocument create = new CreateXmlDocument();
            try
            {
                Client client = new Client();

                client.SendMessageFromSocket(30000, 0);
            }
            catch (Exception e){
                Console.WriteLine(e.Message);

            }

        }
    }
}
