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
                    if (!ServiceClass.ConnectDB(File.ReadAllText(path)))
                    {
                        Console.WriteLine("Не удалось установить соединение!");
                        file.Delete();
                        Environment.Exit(1);
                    }
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

                switch (args[0])
                {
                    // 1. Создание таблицы Person {}
                    case "1":

                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                    case "6":
                        break;
                    default:
                        throw new ArgumentException(nameof(args));
                }

            }
            // Выбрасываем exсeption, т.к. отсутствует параметр командной строки 
            else throw new ArgumentNullException(nameof(args));
        }
    }
}