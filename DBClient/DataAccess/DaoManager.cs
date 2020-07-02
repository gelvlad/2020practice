using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace DBClient.DataAccess
{
    public class DaoManager
    {
        public DbConnection Connection { get; }
        private readonly Dictionary<string, TableDao> daos;

        public DaoManager()
        {

        }

        public TableDao GetDao(string tableName)
        {
            if (daos.ContainsKey(tableName) && daos[tableName] != null)
                return daos[tableName];


        }
    }
}
