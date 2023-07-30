using System.Data;
using System.Globalization;
using System.Text;

namespace myApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Проверяем введен ли параметр коммандной строки
            if (args.Length > 0)
            {
                #region Настройка строки подключения к БД

                // Будем хранить строку подключения в текстовом файле
                // Получаем информацию о файле
                string path = "connection.txt";
                FileInfo file = new FileInfo(path);

                // Проверяем создан ли файл со строкой подключения
                if (file.Exists)
                {
                    // Получаем строку из файла и проверяем подключение,
                    // при неудачном подключении удаляем файл со строкой
                    string connection = File.ReadAllText(path);

                    if (!ServiceClass.ConnectDB(connection))
                    {
                        Console.WriteLine("Не удалось установить соединение!");
                        file.Delete();
                        Environment.Exit(1);
                    }
                    else ServiceClass.connectionString = connection;
                }
                else
                {
                    // Пользователь вводит строку подключения,
                    // при успешном подключении создается файл, 
                    // который содержит в себе валидную строку подключения 
                    while (true)
                    {
                        Console.Write("Введите строку подключения к БД: ");
                        string? connection = Console.ReadLine();

                        if (ServiceClass.ConnectDB(connection))
                        {
                            ServiceClass.connectionString = connection;
                            Console.WriteLine("Соединение установлено!");
                            break;
                        }
                        else Console.WriteLine("Не удалось установить соединение!");
                    }

                    File.AppendAllText(path, ServiceClass.connectionString);
                }
                #endregion

                // Проверяем корректность параметров. Только 2 пункт принимает больше 1 аргумента (4)
                if (args[0] == "2" && args.Length != 4) throw new ArgumentException(nameof(args));

                switch (args[0])
                {
                    // 1. Создание таблицы Person {Id int primary key, Fullname: nvarchar(50), Date_birthday: date, Gender: nvarchar(6)}
                    // Пример входных данных: myApp 1
                    case "1":
                        // Вызываем метод, который создает/пересоздает таблицу Person и возвращает bool-значение в зависимости от результата
                        if (ServiceClass.CreateTable()) Console.WriteLine("Таблица Person успешно создана!");
                        else throw new Exception("Не удалось создать таблицу!");
                        break;

                    // 2. Добавление записи в таблицу Person. 
                    //    Пример входных данных: myApp 2 Ivan 01.01.2000 Male
                    //                         : myApp 2 Maria 06.06.2001 Female
                    case "2":
                        // Проверяем корректность параметров
                        if (args[1].Length > 50) throw new ArgumentException(nameof(args));
                        if (!DateTime.TryParse(args[2], out DateTime date)) throw new ArgumentException(nameof(args));
                        if (args[3] != "Male" && args[3] != "Female") throw new ArgumentException(nameof(args));

                        // Вызываем метод, который добавляет запись и возвращает bool-значение
                        if (ServiceClass.InsertRecord(args[1], date.ToString("yyyy-MM-dd"), args[3])) Console.WriteLine("Запись добавлена!");
                        else throw new Exception("Не удалось добавить запись!");
                        break;

                    // 3. Вывод записей с уникальным значением полей (Fullname, Date_birthday), отсортированных по Fullname,
                    // вывести Fullname, Date_birthday, Gender, Age (кол-во полных лет).
                    // Пример входных данных: myApp 3
                    case "3":
                        // Вызываем метод, который выполняет запрос на выборку данных и возвращает объект DataTable
                        var data = ServiceClass.SelectUniqueRecords();
                        if (data != null)
                        {
                            // Выводим данные в консоль
                            Console.WriteLine("{0, -30} {1, -20} {2, -10} {3, 0}", "ФИО", "Дата рождения", " " + "Пол", "Возраст");

                            foreach (DataRow row in data.Rows) 
                                Console.WriteLine("{0, -30} {1, -20} {2, -10} {3, 0}", row[0], " " + ((DateTime) row[1]).ToShortDateString(), row[2], "  " + row[3]);
                        }
                        else throw new NullReferenceException(nameof(data));
                        break;

                    // 4. Вставка миллиона записей...
                    // Пример входных данных: myApp 4
                    case "4":
                        Console.WriteLine("Загрузка...");

                        // Вызываем метод, который выполняет запрос на добавление ~ 1 млн. записей...
                        if (ServiceClass.InsertMillionRecords()) Console.WriteLine("Добавление строк успешно завершено!");
                        else throw new Exception("Что-то полшло не так!");
                        break;

                    // 5. Выборка по критерию (WHERE Fullname LIKE 'F%' AND Gender = 'Male')
                    case "5":
                        // Вызываем метод, который выполняет запрос на выборку данных и возвращает объект DataTable
                        var tuple = ServiceClass.SelectLikeRecords();

                        if (tuple.table != null) Console.WriteLine($"~ Время выполнения запроса: {tuple.time} ms; Количество строк: {tuple.table.Rows.Count}");
                        else throw new NullReferenceException(nameof(tuple));
                        break;

                    // 6. Оптимизация запроса с помощью создания индекса
                    case "6":
                        // Вызываем метод, который создает/удаляет индекс
                        string? result = ServiceClass.OptimizeQuery();

                        if (result != null)
                        {
                            if (result == "Create") Console.WriteLine("Индекс успешно создан!");
                            else Console.WriteLine("Индекс удален!");
                        }
                        else throw new NullReferenceException(nameof(result));
                        break;
                    default:
                        throw new ArgumentException(nameof(args));
                }
            }
            // Выбрасываем exсeption, т.к. отсутствуют входные параметры args
            else throw new ArgumentNullException(nameof(args));
        }
    }
}