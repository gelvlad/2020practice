using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DBClient.Interface;

namespace DBClient
{
    class Program
    {        
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Start();
        }
    }
}
