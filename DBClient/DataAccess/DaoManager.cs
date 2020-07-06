using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Npgsql;

namespace DBClient.DataAccess
{
    public class DaoManager : IDisposable
    {
        public NpgsqlConnection Connection { get; }
        private readonly Dictionary<string, TableDao> daos = new Dictionary<string, TableDao>();

        public DaoManager(string host, string username, string password, string databaseName)
        {
            NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Username = username,
                Password = password,
                Database = databaseName,
                Pooling = true
            };

            NpgsqlConnection connection = new NpgsqlConnection(sb.ToString());
            Connection = connection;
            Connection.Open();
        }

        public TableDao GetDao(string tableName)
        {
            if (daos.ContainsKey(tableName) && daos[tableName] != null)
                return daos[tableName];

            TableDao dao = new TableDao(Connection, tableName);
            daos.Add(tableName, dao);
            return daos[tableName];
        }

        public void NewTable(string tableName, IOrderedDictionary columns)
        {
            TableDao dao = TableDao.CreateTable(Connection, tableName, columns);
            daos.Add(tableName, dao);
        }

        public void NewTable(string tableName, IOrderedDictionary columns, KeyValuePair<string, string> primaryKeyColumn)
        {
            TableDao dao = TableDao.CreateTable(Connection, tableName, columns, primaryKeyColumn);
            daos.Add(tableName, dao);
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
