using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient.Interface
{
    public static class StringExtensions
    {
        public static string EscapeName(this string str) =>
            '"' + str.Replace("\"", "") + '"';

        public static string EscapeValue(this string str) =>
            "'" + str.Replace("'", "\\'") + "'";
    }
}
