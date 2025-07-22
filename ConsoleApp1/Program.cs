using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string connectionString = "Server=DESKTOP-5L65RAI;Uid=username;Pwd=password;";
            //DESKTOP - 5L65RAI
            string connectionString = @"Data Source=DESKTOP-5L65RAI;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Запрос к системному представлению
                string query = "SELECT name, database_id, create_date FROM sys.databases ORDER BY name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Список баз данных:");
                        Console.WriteLine("{0,-30} {1,-10} {2}", "Имя", "ID", "Дата создания");

                        while (reader.Read())
                        {
                            Console.WriteLine("{0,-30} {1,-10} {2}",
                                reader["name"],
                                reader["database_id"],
                                reader["create_date"]);
                        }
                    }
                }
            }
        Console.ReadLine();
    }
    }
}
