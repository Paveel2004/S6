using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Server
{
   
    public class DataBaseHelper
    {
        public static string connectionString { get; set; }
        public static void Query(string query, string connectionString)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public static void Query(string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
