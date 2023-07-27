using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace myApp
{
    static class ServiceClass
    {
        // Строка подключения
        public static string? connectionString = null;

        // Метод проверяет валидность строки подключения к БД
        public static bool ConnectDB(string? connectionString)
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);

                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                        return true;
                    }
                    else return false;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
    }
}
