using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

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

                    if (connection.State == ConnectionState.Open)
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

        // Создание таблицы Person
        public static bool CreateTable()
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        var query = new SqlCommand();
                        query.Connection = connection;

                        query.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Person'";
                        var reader = query.ExecuteReader();

                        string createPerson = "CREATE TABLE Person (Id int primary key identity, Fullname nvarchar(50), Date_birthday date, Gender nvarchar(6))";

                        if (reader.HasRows) query.CommandText = "DROP TABLE Person;" + createPerson;
                        else query.CommandText = createPerson;

                        reader.Close();

                        query.ExecuteNonQuery();

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

        // Добавление записи в таблицу Person
        public static bool InsertRecord(string fullname, string date, string gender)
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        var query = new SqlCommand($"INSERT INTO Person VALUES ('{fullname}', '{date}', '{gender}')", connection);
                        query.ExecuteNonQuery();

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

        // Выборка уникальных значений по (Fullname + Date_birthday), отсорт. по Fullname
        public static DataTable? SelectUniqueRecords()
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        var query = new SqlCommand($"SELECT p.Fullname AS 'ФИО', p.Date_birthday AS 'Дата рождения', p.Gender AS 'Пол', " +
                        $"DATEDIFF(yy, p.Date_birthday, GETDATE()) - CASE WHEN (MONTH(p.Date_birthday) > MONTH(GETDATE())) OR (MONTH(p.Date_birthday) = MONTH(GETDATE()) AND DAY(p.Date_birthday) > DAY(GETDATE())) THEN 1 ELSE 0 END AS 'Возраст'" +
                        $"FROM (SELECT  *, ROW_NUMBER() OVER(PARTITION BY Fullname, Date_birthday ORDER BY Fullname) rn FROM Person) p WHERE rn = 1 ORDER BY Fullname", connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(query);
                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        connection.Close();

                        return data.Tables[0];
                    }
                    else return null;
                }
                catch
                {
                    return null;
                }
            }
            else return null;
        }

        // Вставка ~ миллиона записей в таблицу Person
        public static bool InsertMillionRecords()
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);

                    var adapter = new SqlDataAdapter("SELECT * FROM Person", connection);
                    var data = new DataTable();
                    adapter.FillSchema(data, SchemaType.Source);

                    string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                          "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                    Random ran = new Random();

                    for (int i = 0; i < 1_000_000; i += alphabet.Length)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            var row = data.NewRow();

                            row["Fullname"] = alphabet[j];
                            row["Date_birthday"] = DateTime.Now.ToString("yyyy-MM-dd");
                            row["Gender"] = ran.Next(0, 2) == 0 ? "Male" : "Female";

                            data.Rows.Add(row);
                        }
                    }

                    string[] names = { "Fedor", "Fabricio", "Fred", "Fidel", "Florence", "Frank", "Felix" };

                    for(int i = 0; i < 100; i++)
                    {
                        var row = data.NewRow();

                        row["Fullname"] = names[ran.Next(0, names.Length)];
                        row["Date_birthday"] = DateTime.Now.ToString("yyyy-MM-dd");
                        row["Gender"] = "Male";

                        data.Rows.Add(row);
                    }

                    data.AcceptChanges();

                    var bulk = new SqlBulkCopy(connection) { DestinationTableName = "Person" };

                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        bulk.WriteToServer(data);

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

        // Выборка значений, где Fullname начинается с 'F' (LIKE 'F%') и Gender = 'Male' 
        public static (DataTable? table, long time) SelectLikeRecords()
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        var query = new SqlCommand($"SELECT * FROM Person WHERE Fullname LIKE 'F%' AND Gender = 'Male'", connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(query);
                        DataSet data = new DataSet();

                        var stopwatch = new Stopwatch();
                        
                        stopwatch.Start();
                        adapter.Fill(data);
                        stopwatch.Stop();

                        connection.Close();
                        return (data.Tables[0], stopwatch.ElapsedMilliseconds);
                    }
                    else return (null, 0);
                }
                catch
                {
                    return (null, 0);
                }
            }
            else return (null, 0);
        }

        // Создание/удаление индекса в таблице Person
        public static string? OptimizeQuery()
        {
            if (connectionString != null)
            {
                try
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        var query = new SqlCommand($"SELECT * FROM sys.indexes WHERE name = 'IX_Person'", connection);
                        SqlDataReader reader = query.ExecuteReader();

                        string result = "";
                        if (reader.HasRows)
                        {
                            query.CommandText = "DROP INDEX IX_Person on Person";
                            result = "Delete";
                        }
                        else
                        {
                            query.CommandText = "CREATE INDEX IX_Person ON Person (Fullname, Gender) INCLUDE (Date_birthday) WHERE Gender = 'Male'";
                            result = "Create";
                        }

                        reader.Close();

                        query.ExecuteNonQuery();

                        connection.Close();
                        return result;
                    }
                    else return null;
                }
                catch
                {
                    return null;
                }
            }
            else return null;
        }
    }
}
