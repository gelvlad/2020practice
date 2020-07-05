using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBClient.DataAccess
{
    public class TableDao
    {
        public string TableName { get; }
        private readonly DbConnection connection;
        private readonly HashSet<string> columns;
        private readonly List<string> primaryKeyNames;

        public TableDao(DbConnection connection, string tableName) 
        {
            TableName = tableName;
            this.connection = connection;
            primaryKeyNames = new List<string>();
            columns = new HashSet<string>();

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    $"SELECT a.attname " +
                    $"FROM pg_index i " +
                    $"JOIN pg_attribute a ON a.attrelid = i.indrelid " +
                    $"AND a.attnum = ANY(i.indkey) " +
                    $"WHERE i.indrelid = '{TableName}'::regclass " +
                    $"AND i.indisprimary;";
                command.Prepare();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            primaryKeyNames.Add(reader.GetString(0));
                        }
                        catch (InvalidCastException e)
                        {
                            throw new InvalidCastException($"Got \"{reader.GetFieldType(0)}\" " +
                                $"instead of \"{typeof(string)}\" when reading a name of a primary key column.", e);
                        }
                    }
                }

                command.CommandText = $"SELECT * FROM {TableName} where false";
                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataRowCollection columnsInfo = reader.GetSchemaTable().Rows;
                    foreach (DataRow row in columnsInfo)
                    {
                        try
                        {
                            columns.Add((string)row["ColumnName"]);
                        }
                        catch (InvalidCastException e)
                        {
                            throw new InvalidCastException($"Got \"{row["ColumnName"].GetType()}\" " +
                                $"instead of \"{typeof(string)}\" when reading a name of a column.", e);
                        }
                    }
                }
            }
        }

        public static TableDao CreateTable(DbConnection connection, string tableName, IOrderedDictionary columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            string id = "ID";
            if (columns.Contains(id))
            {
                Random random = new Random();
                string randomizedId;
                for (int i = 0; i < 100; i++)
                {
                    randomizedId = id + random.Next(10000, 100000).ToString();
                    if (!columns.Contains(randomizedId))
                    {
                        id = randomizedId;
                        break;
                    }
                }
            }
            return CreateTable(connection, tableName, columns, new OrderedDictionary { { id, "SERIAL" } });
        }

        public static TableDao CreateTable(DbConnection connection, string tableName,
            IOrderedDictionary columns, IOrderedDictionary customPrimaryKeys)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CREATE TABLE {0} (", tableName);
            foreach (DictionaryEntry primaryKey in customPrimaryKeys)
                sb.AppendFormat("{0} {1} PRIMARY KEY", primaryKey.Key, primaryKey.Value);
            foreach (DictionaryEntry column in columns)
                sb.AppendFormat(", {0} {1}", column.Key, column.Value);
            sb.Append(");");
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
            }
            return new TableDao(connection, tableName);//, columns.Keys, customPrimaryKey.Key);
        }
        //catch (Npgsql.PostgresException e)
        //{
        //    switch (e.SqlState)
        //    {
        //        case "42P07":
        //            Console.WriteLine("Ошибка: такая таблица уже существует");
        //            break;
        //        default:
        //            Console.WriteLine("Ошибка: неизвестная ошибка в SQL");
        //            break;
        //    }
        //    throw;
        //}
        //catch (Npgsql.NpgsqlException e)
        //{
        //    Console.WriteLine("2");
        //}
        //catch (DbException e)
        //{
        //    Console.WriteLine("3");
        //}


        //public void DropTable()
        //{
        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText = $"DROP TABLE {TableName}";
        //        command.ExecuteNonQuery();
        //    }
        //}

        //public void InsertRow(Dictionary<string, object> values)
        //{
        //    // INSERT INTO tablename (col1, col2) VALUES (val1, val2);
        //    var sb = new StringBuilder();
        //    sb.AppendFormat("INSERT INTO {0} (", TableName);
        //    foreach (var name in columnNames)
        //    {
        //        if (!Columns.Contains(name))
        //            throw new IOException($"Column {name} does not exist for this DAO");

        //        sb.AppendFormat("{0}, ", name);
        //    }
        //    sb.Length -= 2;
        //    sb.Append(") VALUES (");
        //    //escape E V E R Y T H I N G
        //    foreach (var value in values)
        //        sb.AppendFormat("{0}, ", value);
        //    sb.Length -= 2;
        //    sb.Append(");");

        //    using (DbCommand command = connection.CreateCommand())
        //    {
        //        command.CommandText = sb.ToString();
        //    }
        //}

        //public void UpdateRow(object primaryKeyValue, IEnumerable<string> columnNames, IEnumerable<string> values)
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
