using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSD_CLIENT_AQUA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Client client  = new Client();
            client.SendMessageFromSocket(Encoding.UTF8.GetBytes("msg"), 49153);
        }
    }
}
