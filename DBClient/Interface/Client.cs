using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DBClient.Interface
{
    public class Client
    {
        public DataAccess.DaoManager Manager { get; private set; }

        public void Start()
        {
            string[] input = null;

            while (input == null || input.Length == 0 || input[0] != "quit")
            {
                if (Manager == null)
                    Console.WriteLine("Подключение не найдено. Для работы с СУБД, подключитесь командой:\n" +
                        "startconnection <hostname> <username> <password> <database>");

                input = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (input.Length == 0)
                    continue;

                switch (input[0])
                {
                    case "startconnection":
                        StartConnection(input);
                        break;
                    case "createtable":
                        CreateTable(input);
                        break;
                    case "droptable":
                        DropTable(input);
                        break;
                    case "insertrow":
                        InsertRow(input);
                        break;
                    case "updaterow":
                        UpdateRow(input);
                        break;
                    case "deleterow":
                        DeleteRow(input);
                        break;
                    case "selectrows":
                        SelectRows(input);
                        break;
                    case "help":
                        ListHelp();
                        break;
                    default:
                        Console.WriteLine();
                        break;
                }
            }
        }
        
        public void StartConnection(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length != 5)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            try
            {
                Manager = new DataAccess.DaoManager(args[1], args[2], args[3], args[4]);
            }
            catch (IOException)
            {
                Console.WriteLine("Сервер не отвечает");
                return;
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine("Подключение к СУБД установлено.");
        }

        public void CreateTable(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 2)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            OrderedDictionary columns = new OrderedDictionary();
            string pkName = null;
            string pkType = null;
            for (int i = 2; i < args.Length; i++)
            {
                string[] columnArg = args[i].Split(':');
                if (columnArg.Length != 2)
                {
                    Console.WriteLine("Не удалось прочитать описание столбца.");
                    return;
                }
                if (columnArg[1].Contains(')'))
                {
                    Console.WriteLine($"Неверный тип \"{columnArg[1]}\"");
                }

                if (pkName == null && columnArg[0].StartsWith("-pk"))
                {
                    pkName = columnArg[0].Substring(3).EscapeName();
                    pkType = columnArg[1];
                }
                else
                {
                    columns.Add(columnArg[0].EscapeName(), columnArg[1]);
                }
            }

            try
            {
                if (pkName == null)
                    Manager.NewTable(args[1], columns);
                else
                    Manager.NewTable(args[1], columns, new KeyValuePair<string, string>(pkName, pkType));
            }
            catch (Npgsql.PostgresException e)
            {
                switch (e.SqlState)
                {
                    case "42P07":
                        Console.WriteLine("Ошибка: такая таблица уже существует.");
                        break;
                    default:
                        Console.WriteLine("Ошибка: " + e.MessageText);
                        break;
                }
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine($"Создана таблица {args[1]}.");
        }

        public void DropTable(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 2)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            try
            {
                Manager.GetDao(args[1]).DropTable();
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine($"Удалена таблица {args[1]}.");
        }

        public void InsertRow(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 2)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            Dictionary<string, string> values = new Dictionary<string, string>();
            for (int i = 2; i < args.Length; i++)
            {
                string[] columnArg = args[i].Split('=');
                if (columnArg.Length != 2)
                {
                    Console.WriteLine("Не удалось прочитать значение столбца.");
                    return;
                }
                values.Add(columnArg[0].EscapeName(), columnArg[1].EscapeValue());
            }
            try
            {
                Manager.GetDao(args[1]).InsertRow(values);
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine($"В таблицу {args[1]} добавлена запись.");
        }

        public void UpdateRow(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 3)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            if (!args[2].StartsWith("-pk"))
            {
                Console.WriteLine("Укажите значение Ключа.");
                return;
            }

            Dictionary<string, string> values = new Dictionary<string, string>();
            for (int i = 2; i < args.Length; i++)
            {
                string[] columnArg = args[i].Split('=');
                if (columnArg.Length != 2)
                {
                    Console.WriteLine("Не удалось прочитать значение столбца.");
                    return;
                }
                values.Add(columnArg[0].EscapeName(), columnArg[1].EscapeValue());
            }
            try
            {
                Manager.GetDao(args[1]).UpdateRow(args[2].EscapeValue(), values);
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine($"В таблице {args[1]} обновлена запись с ключом '{args[2]}'.");
        }

        public void DeleteRow(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 3)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            if (!args[2].StartsWith("-pk"))
            {
                Console.WriteLine("Укажите значение Ключа.");
                return;
            }
            try
            {
                Manager.GetDao(args[1]).DeleteRow(args[2].EscapeValue());
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            Console.WriteLine($"Из таблицы {args[1]} удалена запись с ключом '{args[2]}'.");
        }

        public void SelectRows(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 3)
            {
                Console.WriteLine("Неправильное количество аргументов.");
                return;
            }
            string[] columnArg = args[2].Split('=');
            if (columnArg.Length != 2)
            {
                Console.WriteLine("Не удалось прочитать значение столбца.");
                return;
            }

            List<object[]> result = null;
            try
            {
                result = Manager.GetDao(args[1]).SelectRows(new KeyValuePair<string, string>(columnArg[0].EscapeName(), columnArg[1].EscapeValue()));
            }
            catch (Npgsql.PostgresException e)
            {
                Console.WriteLine("Ошибка: " + e.MessageText);
                return;
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                return;
            }

            foreach (object[] row in result)
            {
                Console.Write('|');
                Console.WriteLine(string.Concat(row.Select(i => $" {i,10} |")));
            }
        }

        public void ListHelp()
        {
            Console.Write(
                "Список возможных комманд:\n" +
                "startconnection <hostname> <username> <password> <database>\n" +
                "createtable <tablename> [-pk<keyname>:<type>] <columnname>:<columntype> <columnname>:<columntype> ...\n" +
                "droptable <tablename>\n" +
                "insertrow <tablename> <columnname>=<value> <columnname>=<value> ...\n" +
                "updaterow <tablename> -pk<keyvalue> <columnname>=<value> <columnname>=<value> ...\n" +
                "deleterow <tablename> -pk<keyvalue>\n" +
                "selectrows <tablename> <columnname>=<value>\n");
        }
    }
}
