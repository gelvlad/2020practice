using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace DBClient
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    System.Threading.Thread.Sleep(5000);

        //    MainAsync(args).GetAwaiter().GetResult();
        //}

        static void Main(string[] args)
        {
            var a = new DataAccess.DaoManager(
                host: "rogue.db.elephantsql.com",
                username: "dswlpdrj",
                password: "mPqliIo75EzrBFoSAhZwYZV47IgOxYCE",
                databaseName: "dswlpdrj");
            string inp = "create table -n\"Table\" -pk\"ID\" \"SERIAL\" -c\"text\" \"varchar(255)\", \"name\" \"varchar(12)\"";

            Console.ReadLine();
        }
    }
}
