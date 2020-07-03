using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;


namespace DBClient.DataAccess
{
    public class TableDao
    {
        public string TableName { get; }
        public HashSet<string> Columns { get; }
        private readonly DbConnection connection;
        private readonly string primaryKeyName;
    
        public TableDao(DbConnection connection, string tableName)
        {
            TableName = tableName;
            this.connection = connection;
            this.primaryKeyName = primaryKeyName;

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM ";
            }
        }

        public static TableDao CreateTable(DbConnection connection, string tableName, IEnumerable<(string, string)> columns) =>
            CreateTable(connection, tableName, columns, ("ID", "SERIAL"));

        public static TableDao CreateTable(DbConnection connection, string tableName,
            IEnumerable<(string name, string type)> columns, (string name, string type) primaryKeyColumn)
        {
            StringBuilder columnsSB = new StringBuilder();
            foreach (var (name, type) in columns)
                columnsSB.AppendFormat(", {0} {1}", name, type);
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    $"CREATE TABLE {tableName} (" +
                    $"{primaryKeyColumn.name} {primaryKeyColumn.type} PRIMARY KEY" +
                    $"{columnsSB})";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException e)
                {
                    switch (e.SqlState)
                    {
                        case "42P07":
                            Console.WriteLine("Ошибка: такая таблица уже существует");
                            break;
                        default:
                            Console.WriteLine("Ошибка: неизвестная ошибка в SQL");
                            break;
                    }
                    throw;
                }
                catch (Npgsql.NpgsqlException e)
                {
                    Console.WriteLine("2");
                }
                catch (DbException e)
                {
                    Console.WriteLine("3");
                }
            }
            return new TableDao(connection, tableName);//, primaryKeyColumn.name);
        }

        public void DropTable()
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DROP TABLE {TableName}";
                command.ExecuteNonQuery();
            }
        }

        //public void InsertRow(IEnumerable<string> columnNames, IEnumerable<string> values)
        //{
        //    // insert row -tsometable col1=asd col2=dsa col3=3

        //    foreach (var name in columnNames)
        //    {
        //        if (!Columns.Contains(name))
        //        {
        //            //no column pepehands
        //        }
        //    }

        //    //escape E V E R Y T H I N G
        //    var sb = new StringBuilder();
        //    foreach (var value in values)
        //    {

        //    }

        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText =
        //            $"INSERT INTO {TableName} ({columnNames}) " +
        //            $"VALUES ({values})";
        //    }
        //}

        //public void UpdateRow()
        //{
        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText =
        //            $"INSERT INTO {TableName} ({columnNames}) " +
        //            $"VALUES ({values})";
        //    }
        //}

        //public void DeleteRow()
        //{
        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText =
        //            $"INSERT INTO {TableName} ({columnNames}) " +
        //            $"VALUES ({values})";
        //    }
        //}

        //public void SelectRows()
        //{
        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText =
        //            $"INSERT INTO {TableName} ({columnNames}) " +
        //            $"VALUES ({values})";
        //    }
        //}
    }
}
