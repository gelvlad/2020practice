using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient.DataAccess
{
    public class PostgresTableDao : TableDao
    {
        public PostgresTableDao(DbConnection connection, string tableName) : base(connection, tableName) { }

        public override string QueryIdColumn()
        {
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
                string name;
                try
                {
                    name = (string)result;
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException($"Got \"{result.GetType()}\" " +
                        $"instead of \"{typeof(string)}\" when reading a name of a primary key.", e);
                }

                return name;
            }
        }
    }
}
