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
            NpgsqlConnection connection = new NpgsqlConnection(
                "Host=rogue.db.elephantsql.com;" +
                "Username=dswlpdrj;" +
                "Password=mPqliIo75EzrBFoSAhZwYZV47IgOxYCE;" +
                "Database=dswlpdrj");

            connection.Open();
            string inp = "create table -n\"Table\" -pk\"ID\" \"SERIAL\" -c\"text\" \"varchar(255)\", \"name\" \"varchar(12)\"";


            DataAccess.TableDao.CreateTable(connection, "\\\"newtable", new (string, string)[] { ("text", "varchar(255)"), ("name", "varchar(12)") });

            Console.ReadLine();
        }
    }
}
