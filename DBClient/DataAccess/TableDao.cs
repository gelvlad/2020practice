﻿using System;
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
        private readonly string primaryKeyName;

        public TableDao(DbConnection connection, string tableName) 
        {
            TableName = tableName;
            this.connection = connection;

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

                object result = command.ExecuteScalar();
                try
                {
                    primaryKeyName = (string)result;
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException($"Got \"{result.GetType()}\" " +
                        $"instead of \"{typeof(string)}\" when reading a name of a primary key.", e);
                }
            }
        }

        public static TableDao CreateTable(DbConnection connection, string tableName, IOrderedDictionary columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }
            Random random = new Random();
            string id = "ID_AUTOGENERATED" + random.Next(10000, 100000).ToString();
            return CreateTable(connection, tableName, columns, new KeyValuePair<string, string>(id, "SERIAL"));
        }

        public static TableDao CreateTable(DbConnection connection, string tableName,
            IOrderedDictionary columns, KeyValuePair<string, string> customPrimaryKey)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CREATE TABLE {0} (", tableName);
            sb.AppendFormat("{0} {1} PRIMARY KEY", customPrimaryKey.Key, customPrimaryKey.Value);
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

        public void DropTable()
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DROP TABLE {TableName}";
                command.ExecuteNonQuery();
            }
        }

        public void InsertRow(Dictionary<string, object> values)
        {
            StringBuilder namesSB = new StringBuilder();
            StringBuilder valuesSB = new StringBuilder();
            foreach (KeyValuePair<string, object> value in values)
            {
                namesSB.AppendFormat("{0}, ", value.Key);
                valuesSB.AppendFormat("{0}, ", value);
            }
            namesSB.Length -= 2;
            valuesSB.Length -= 2;
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO {TableName} ({namesSB}) VALUES ({valuesSB});";
                command.ExecuteNonQuery();
            }
        }

        public void UpdateRow(string primaryKeyValue, IDictionary<string, string> values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> value in values)
            {
                sb.AppendFormat("{0} = {1}, ", value.Key, value.Value);
            }
            sb.Length -= 2;

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"UPDATE {TableName} SET ({sb}) WHERE {primaryKeyName} = {primaryKeyValue};";
                command.ExecuteNonQuery();
            }
        }

        public void DeleteRow(string primaryKeyValue)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"DELETE FROM {TableName} WHERE {primaryKeyName} = {primaryKeyValue}";
                command.ExecuteNonQuery();
            }
        }

        public List<object[]> SelectRows(KeyValuePair<string, string> where)
        {
            List<object[]> rows = new List<object[]>();
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM {TableName} WHERE {where.Key} = {where.Value}";
                using (DbDataReader reader = command.ExecuteReader())
                {
                    DataRowCollection schemaRows = reader.GetSchemaTable().Rows;
                    object[] row = new object[schemaRows.Count];
                    for (int i = 0; i < schemaRows.Count; i++)
                        row[i] = schemaRows[i][0];
                    rows.Add(row);
                    while (reader.Read())
                    {
                        row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        rows.Add(row);
                    }
                }
            }

            return rows;
        }
    }
}
