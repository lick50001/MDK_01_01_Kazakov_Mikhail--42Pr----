using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopContent.Classes
{
    public class Connection
    {
        private static readonly string config = "server=localhost;database=ShopContent;port=3306;uid=root;pwd=;";

        public static SqlConnection OpenConnection()
        {
            SqlConnection conn = new SqlConnection(config);
            conn.Open();
            return conn;
        }

        public static SqlDataReader Query(string sql, out SqlConnection conn)
        {
            conn = OpenConnection();
            return new SqlCommand(sql, conn).ExecuteReader();
        }

        public static SqlConnection CloseConnection(SqlConnection connection)
        {
            connection.Close();
            SqlConnection.ClearPool(connection);
        }
    }
}
