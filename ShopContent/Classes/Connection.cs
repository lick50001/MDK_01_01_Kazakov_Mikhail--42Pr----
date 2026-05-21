using MySql.Data.MySqlClient;

namespace ShopContent.Classes
{
    public class Connection
    {
        private static readonly string config = "server=localhost;database=ShopContent;uid=root;pwd=;";

        public static MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection(config);
            conn.Open();
            return conn;
        }

        public static MySqlDataReader Query(string sql, out MySqlConnection conn)
        {
            conn = OpenConnection();
            return new MySqlCommand(sql, conn).ExecuteReader();
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            connection.Close();
            MySqlConnection.ClearPool(connection);
        }
    }
}
