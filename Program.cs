using System.Data;
using System.Data.SQLite;

namespace Game
{
    class EscapeRoomQuest
    {
        static SQLiteConnection connection;
        static SQLiteCommand command;

        static long sessionId;
        // Получение правильного ответа по ID уровня
        static public double GetCorrectAnswerByLevelID(int levelId)
        {
            try
            {
                command.CommandText = "SELECT answer FROM task WHERE level_id = @levelId";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@levelId", levelId);
                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDouble(result);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении ответа: {ex.Message}");
                return 0;
            }
        }
        static public bool Connect(string fileName)
        {
            try
            {
                connection = new SQLiteConnection("Data Source=" + fileName + ";Version=3; FailIfMissing=False");
                connection.Open();
                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
                return false;
            }
        }
        // Получение типа задачи по ID
        static public string GetTaskTypeByTaskId(int taskId)
        {
            try
            {
                command.CommandText = "SELECT task_type FROM task WHERE level_id = " + taskId;
                object result = command.ExecuteScalar();
                return result?.ToString() ?? "Неизвестно";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении типа задачи: {ex.Message}");
                return "Ошибка";
            }
        }
        static public string GetHintBytaskId(int taskId)
        {
            try
            {
                command.CommandText = "SELECT task_type FROM task WHERE level_id = " + taskId;
                object result = command.ExecuteScalar();
                return result?.ToString() ?? "Неизвестно";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении типа задачи: {ex.Message}");
                return "Ошибка";
            }
        }
        // Получение приветственного сообщения по ID
        static public string GetGreetingByTaskId(int taskId)
        {
            try
            {
                command.CommandText = "SELECT name FROM task WHERE level_id = " + taskId;
                object result = command.ExecuteScalar();
                return result?.ToString() ?? "Приветственное сообщение недоступно";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении приветствия: {ex.Message}");
                return "Ошибка при загрузке приветствия";
            }
        }
        // Получение условия задачи по ID
        static public string GetConditionByTaskId(int taskId)
        {
            try
            {
                command.CommandText = "SELECT description FROM task WHERE level_id = " + taskId;
                object result = command.ExecuteScalar();
                return result?.ToString() ?? "Условие задачи недоступно";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении условия задачи: {ex.Message}");
                return "Ошибка при загрузке условия задачи";
            }
        }

        // Получение подсказки по ID
        // Получение подсказки по ID уровня
        static public string GetHintByTaskId(int levelId)
        {
            try
            {
                // Используем параметризованный запрос для безопасности
                command.CommandText = "SELECT hint FROM task WHERE level_id = @levelId";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@levelId", levelId);

                object result = command.ExecuteScalar();

                // Проверяем результат и возвращаем подсказку или сообщение об ошибке
                if (result != null && result != DBNull.Value)
                {
                    return result.ToString();
                }
                else
                {
                    return "Подсказка недоступна для этого уровня";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении подсказки: {ex.Message}");
                return "Ошибка при загрузке подсказки";
            }
        }


        static public string GetEggByLevelID(int level)
        {
            command.CommandText = "SELECT name FROM egg WHERE level_id = " + level;
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            if (data.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return data.Rows[0].Field<string>("name");
            }
        }

        static public long GetEggIDByLevelID(int level)
        {
            command.CommandText = "SELECT id FROM egg WHERE level_id = " + level;
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            if (data.Rows.Count == 0)
            {
                throw new Exception();
            }
            else
            {
                return data.Rows[0].Field<long>("id");
            }
        }

        static public long GetEggCountBySessionID(long session)
        {
            command.CommandText = "SELECT COUNT(*) FROM egg_progress WHERE session_id = " + session;
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            if (data.Rows.Count == 0)
            {
                throw new Exception();
            }
            else
            {
                return data.Rows[0].Field<long>(0);
            }
        }

        static public int GetCurrentLevelBySessionID(long session)
        {
            command.CommandText = "SELECT level_id FROM task WHERE id = (SELECT MAX(task_id) FROM task_progress WHERE session_id = " + session + ")";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            if (data.Rows.Count == 0)
            {
                return -1;
            }
            else
            {
                return (int)data.Rows[0].Field<long>(0);
            }
        }

        static public void CollectEgg(long eggId, long sessionId)
        {
            command.CommandText = "INSERT INTO egg_progress (session_id, egg_id) VALUES (" + sessionId + "," + eggId + ");";
            command.ExecuteNonQuery();
        }

        static public void CompleteLevel(long levelId, long sessionId)
        {
            command.CommandText = "INSERT INTO task_progress (session_id, task_id) VALUES (" + sessionId + ", (SELECT id FROM task WHERE level_id = " + levelId + "));";
            command.ExecuteNonQuery();
        }

        static public bool IsEggProgressWithSessionIDExists(long eggId, long sessionId)
        {
            command.CommandText = "SELECT * FROM egg_progress WHERE session_id = " + sessionId + " AND egg_id = " + eggId;
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            return data.Rows.Count == 1;
        }

        static public bool IsLevelComplete(long levelId, long sessionId)
        {
            command.CommandText = "SELECT * FROM task_progress WHERE session_id = " + sessionId + " AND task_id = (SELECT id FROM task WHERE level_id = " + levelId + ")";
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            return data.Rows.Count == 1;
        }

        // Game — служит центральным хранилищем глобальных переменных игры
        // Все данные хранятся здесь, чтобы быть доступными из любой части программы без создания экземпляров
        static class Game
        {
            // Текущий уровень игры (диапазон от 0 до 8, всего 9 уровней)
            // При старте игры устанавливается в 0, увеличивается по мере прохождения
            public static int CurrentLevel = 0;
            // Флаг режима разработчика, активируется через настройки
            // В этом режиме появляются дополнительные команды для отладки и тестирования
            public static bool DeveloperMode = false;
            // Текущий выбор музыкального сопровождения
            // По умолчанию "Default", может быть изменен в настройках
            public static string Music = "Default";
            // Текущий выбор текстур (визуального стиля)
            // По умолчанию "Default", может быть изменен в настройках
            public static string Textures = "Default";
        }

        // Точка входа в программу - первый метод, который вызывается при запуске приложения
        static void Main()
        {
            if (Connect("escapeRoomBase.sqlite"))
            {
                Console.WriteLine("DB Connected");
                command = new SQLiteCommand(connection);
                InitDbIfNeeded();
            }
            else
            {
                Console.WriteLine("DB connection error");

            }

            // Приветственное сообщение, отображаемое при старте игры
            Console.WriteLine("Добро пожаловать в Escape-Room Quest!");
            // Переход к отображению главного меню
            ShowMainMenu();
        }

        static void InitDbIfNeeded()
        {
            try
            {
                command.CommandText = "SELECT * FROM level";
                DataTable data = new DataTable();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                adapter.Fill(data);
                Console.WriteLine($"Прочитано {data.Rows.Count} записей из таблицы БД");
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Init DB started");

                // создание таблиц
                Console.WriteLine($"Init DB structure");
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS [level]([id] INTEGER PRIMARY KEY NOT NULL UNIQUE, [name] TEXT, [description] TEXT); " +
                    "CREATE TABLE IF NOT EXISTS [egg]([id] INTEGER PRIMARY KEY NOT NULL UNIQUE, [level_id] INTEGER, [name] TEXT); " +
                    "CREATE TABLE IF NOT EXISTS [session]([id] INTEGER PRIMARY KEY NOT NULL UNIQUE, [last_date] TEXT); " +
                    "CREATE TABLE IF NOT EXISTS [task]([id] INTEGER PRIMARY KEY NOT NULL UNIQUE, [level_id] INTEGER, [name] TEXT, [description] TEXT, [answer] TEXT, [hint] TEXT); " +
                    "CREATE TABLE IF NOT EXISTS [task_progress]([session_id] INTEGER, [task_id] INTEGER); " +
                    "CREATE TABLE IF NOT EXISTS [egg_progress]([session_id] INTEGER, [egg_id] INTEGER); " +
                    "CREATE TABLE IF NOT EXISTS [setting]([music_file] TEXT, [texture_file] TEXT, [debug_mode] INTEGER); "; ;
                command.ExecuteNonQuery();

                // заполнение таблиц
                Console.WriteLine($"Init DB data");
                // заполнение сессий
                command.CommandText =
                    "INSERT INTO session (id) VALUES (1); " +
                    "INSERT INTO session (id) VALUES (2); " +
                    "INSERT INTO session (id) VALUES (3);";
                command.ExecuteNonQuery();

                // заполнение пасхалок
                command.CommandText =
                    "INSERT INTO egg (id, level_id, name) VALUES (1, 0, \"Вы нашли поломанную оперативную память! Это первая пасхалка, спрятанная в шкафу у двери!\"); " +
                    "INSERT INTO egg (id, level_id, name) VALUES (2, 3, \"Вы нашли забагованный светильник! Это вторая пасхалка, она мигает в углу комнаты!\"); " +
                    "INSERT INTO egg (id, level_id, name) VALUES (3, 5, \"Вы нашли дрон из телескопа! Это третья пасхалка, он застрял в вентиляции!\"); " +
                    "INSERT INTO egg (id, level_id, name) VALUES (4, 8, \"Вы нашли медный ключ! Это четвертая пасхалка, он лежит под сервером в углу!\"); ";
                command.ExecuteNonQuery();


                // Заполняем задачу 0 (программирование - исследование)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 1);
                command.Parameters.AddWithValue("@level_id", 0);
                command.Parameters.AddWithValue("@name", "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно провести исследование по анализу данных системы безопасности. Это программистская задача, но мои вычислительные мощности повреждены. Поможешь мне написать эффективный алгоритм?");
                command.Parameters.AddWithValue("@description", "ОЛД: *шипение поврежденных проводов* Слушай, я обнаружил поврежденный терминал с критически важными данными. Нам нужно восстановить поврежденные данные, чтобы продолжить побег!\n\nСистема зашифрована, и чтобы разблокировать ее, мне нужно провести исследование.\n\nВот что мне удалось расшифровать:\nВ системе безопасности лаборатории есть 5 ключевых узлов. Каждый узел имеет уникальный идентификатор от 1 до 5. Из-за повреждений некоторые узлы не отвечают, но я знаю, что:\n\n- Если узел 1 работает, то работает и узел 2\n- Узел 3 работает только если не работает узел 4\n- Узел 5 работает только если работают оба узла 1 и 3\n- Если узел 4 работает, то работает и узел 2\n\nМне нужно определить, какие узлы работают, чтобы восстановить систему. Но я не могу проверить каждый вариант вручную — мои вычислительные мощности повреждены!\n\nПомоги мне написать программу, которая найдет все возможные комбинации работающих узлов. Это критически важно для нашего побега — от этого зависит, сможем ли мы обойти систему безопасности!\n\nВведи 5-значное число, где каждая цифра 1 или 0 (1 — узел работает, 0 — не работает), начиная с узла 1 и заканчивая узлом 5. Например, 10101 означает, что работают узлы 1, 3 и 5.");
                command.Parameters.AddWithValue("@answer", "12345");
                command.Parameters.AddWithValue("@hint", "Рассмотрите все возможные варианты и исключите неподходящие");
                command.ExecuteNonQuery();

                // Заполняем задачу 1 (алгебра - охлаждение процессора)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 2);
                command.Parameters.AddWithValue("@level_id", 1);
                command.Parameters.AddWithValue("@name", "ОЛД: *звуки перегревающегося процессора* Эй, дружище! Я чувствую, как мои чипы плавятся! Если я не охладюсь в ближайшие минуты, я отключусь навсегда. Нам нужно срочно решить задачу по охлаждению процессора, чтобы продолжить побег из этой лаборатории!");
                command.Parameters.AddWithValue("@description", "ОЛД: *звуки перегревающегося процессора* Слушай, я чувствую себя ужасно! Мой процессор перегревается, и если я не охладюсь вовремя, я отключусь навсегда! Посмотри на эти данные: 2 минуты назад моя температура была 85°C, а сейчас, через 5 минут после начала охлаждения, она 55°C. Нормальная рабочая температура — 25°C.\n\nМне нужно, чтобы ты помог мне рассчитать, через сколько минут я смогу безопасно запуститься снова, когда температура опустится ниже 40°C. Для этого используй уравнение охлаждения:\nT(t) = 25 + (T₀ - 25)e^(-kt)\n\nГде:\nT(t) — температура в момент времени t\nT₀ — начальная температура\nk — коэффициент охлаждения\n\nПомни, что от этого зависит, сможем ли мы продолжить побег из этой лаборатории! Сначала найди коэффициент охлаждения k и начальную температуру T₀, а потом определи, через сколько минут я смогу безопасно запуститься.");
                command.Parameters.AddWithValue("@answer", "8");
                command.Parameters.AddWithValue("@hint", "Используйте два уравнения с экспонентой для нахождения k и T₀");
                command.ExecuteNonQuery();

                // Заполняем задачу 2 (геометрия - расстояние между точками)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 3);
                command.Parameters.AddWithValue("@level_id", 2);
                command.Parameters.AddWithValue("@name", "ОЛД: *поскребывание по металлической двери* Привет! Я застрял между секторами лаборатории. Передо мной два возможных пути, но я не могу определить безопасный маршрут. Мне нужна твоя помощь с геометрической задачей, чтобы мы могли выбраться отсюда!");
                command.Parameters.AddWithValue("@description", "ОЛД: *поскребывание по металлической двери* Эй, я обнаружил две возможные точки выхода из этого коридора — точка A и точка B. Но чтобы выбрать безопасный путь, мне нужно знать расстояние между ними.\n\nЯ провел сканирование и обнаружил, что от моего текущего положения (точка C) до точки A расстояние 70 метров, а до точки B — 50 метров. Угол между этими направлениями — 120°.\n\nТы помнишь теорему косинусов? Мне нужно точное расстояние между точками A и B, потому что в точке B я обнаружил работающий лифт, а в точке A — охранного дрона. Если я ошибусь в расчетах, мы можем напороться на систему безопасности!\n\nПомоги мне определить расстояние между выходами A и B, чтобы мы могли безопасно выбраться из этого коридора.");
                command.Parameters.AddWithValue("@answer", "104,4");
                command.Parameters.AddWithValue("@hint", "Используйте теорему косинусов для нахождения расстояния между точками");
                command.ExecuteNonQuery();

                // Заполняем задачу 3 (алгебра - тригонометрическое уравнение)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 4);
                command.Parameters.AddWithValue("@level_id", 3);
                command.Parameters.AddWithValue("@name", "ОЛД: *звуки короткого замыкания* Привет! Мои сенсоры показывают странные значения... Я обнаружил панель управления, но чтобы активировать дверь в следующий сектор, мне нужно ввести правильный угол. Поможешь мне решить эту тригонометрическую задачу перед тем, как сработает тревога?");
                command.Parameters.AddWithValue("@description", "ОЛД: *звуки короткого замыкания* Ой! Мои сенсоры показывают странные значения... Я обнаружил панель управления, но чтобы активировать дверь в следующий сектор, мне нужно ввести правильный угол α.\n\nСистема выдала мне два уравнения:\n3sin²α + 2cos²α = 2.5\nи\nsinα * cosα = 0.2\n\nЯ знаю, что угол должен быть между 0° и 90°, но мои поврежденные сенсоры не могут решить эту систему. Если мы введем неправильный угол, сработает тревога!\n\nПомоги мне найти правильное значение угла α. Это наш единственный шанс пройти через эту дверь, прежде чем система безопасности обнаружит нас. Вспомни основные тригонометрические тождества — они тебе пригодятся!");
                command.Parameters.AddWithValue("@answer", "45");
                command.Parameters.AddWithValue("@hint", "Используйте основное тригонометрическое тождество sin²α + cos²α = 1");
                command.ExecuteNonQuery();

                // Заполняем задачу 4 (геометрия - расстояние в пирамиде)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 5);
                command.Parameters.AddWithValue("@level_id", 4);
                command.Parameters.AddWithValue("@name", "ОЛД: *звуки сканирующего лазера* Внимание! Я нашел лазерную систему безопасности, но чтобы обезвредить ее, мне нужно точно знать расстояние. Эта система построена на основе правильной четырехугольной пирамиды. Поможешь рассчитать критически важное расстояние, чтобы мы могли продолжить побег?");
                command.Parameters.AddWithValue("@description", "ОЛД: *звуки сканирующего лазера* Эй, я нашел лазерную систему безопасности, но чтобы обезвредить ее, мне нужно точно знать расстояние. Эта система построена на основе правильной четырехугольной пирамиды с длиной стороны основания 10 метров и высотой 12 метров.\n\nМне нужно найти расстояние от середины бокового ребра пирамиды до середины основания. Это критически важно, потому что именно на этом расстоянии расположена главная точка доступа к системе безопасности. Если я введу неправильное значение, лазеры активируются и отрежут мне путь!\n\nПомоги мне рассчитать это расстояние. Я помню, что нужно использовать теорему Пифагора и свойства правильных пирамид. Будь точен — от этого зависит, сможем ли мы обезвредить лазерную систему и продолжить наш путь!");
                command.Parameters.AddWithValue("@answer", "6,96");
                command.Parameters.AddWithValue("@hint", "Используйте теорему Пифагора в трехмерном пространстве");
                command.ExecuteNonQuery();

                // Заполняем задачу 5 (алгебра - максимизация солнечных панелей)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 6);
                command.Parameters.AddWithValue("@level_id", 5);
                command.Parameters.AddWithValue("@name", "ОЛД: *звуки работающих солнечных панелей* Привет! Я подключился к системе энергоснабжения лаборатории. У нас есть шанс восстановить питание и запустить аварийный лифт, но для этого нужно оптимизировать мощность солнечных панелей. Поможешь найти оптимальное напряжение?");
                command.Parameters.AddWithValue("@description", "ОЛД: *звуки работающих солнечных панелей* Эй, я подключился к системе энергоснабжения лаборатории! У нас есть шанс восстановить питание и запустить аварийный лифт, но для этого нужно оптимизировать мощность солнечных панелей.\n\nЗависимость мощности от напряжения задана формулой: P(V) = 80V - 0.8V²\nгде V — напряжение в вольтах (от 0 до 100).\n\nМне нужно найти напряжение V, при котором мощность будет максимальной. Если мы подадим слишком высокое напряжение, панели перегорят, а слишком низкое — лифт не запустится. Это критически важно для нашего побега!\n\nПомоги мне найти оптимальное напряжение. Я помню, что для нахождения максимума нужно использовать производную, но мои вычислительные мощности сейчас ограничены. Будь осторожен — от этого зависит, сможем ли мы выбраться из лаборатории!");
                command.Parameters.AddWithValue("@answer", "2000");
                command.Parameters.AddWithValue("@hint", "Используйте производную функции мощности для нахождения максимума");
                command.ExecuteNonQuery();

                // Заполняем задачу 6 (программирование - числа с 5 делителями)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 7);
                command.Parameters.AddWithValue("@level_id", 6);
                command.Parameters.AddWithValue("@name", "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно посчитать числа с 5 делителями, чтобы взломать шифр на главном терминале. Это программистская задача, но мои вычислительные мощности повреждены. Поможешь мне написать эффективный алгоритм?");
                command.Parameters.AddWithValue("@description", "ОЛД: *шипение поврежденных проводов* Слушай, мне нужно посчитать числа с 5 делителями, чтобы взломать шифр на главном терминале. Это программистская задача, но мои вычислительные мощности повреждены.\n\nСистема безопасности использует числа, имеющие ровно 5 натуральных делителей. Я знаю, что такие числа очень редки, и мне нужно найти все такие числа в диапазоне от 1 до 10000.\n\nПомоги мне написать эффективный алгоритм для поиска этих чисел. Подсказка: числа с 5 делителями имеют особую структуру — они представляют собой четвертую степень простого числа.\n\nНапример, число 16 имеет делители: 1, 2, 4, 8, 16 — всего 5 делителей.\n\nВведи количество таких чисел в указанном диапазоне. Это критически важно для нашего побега!");
                command.Parameters.AddWithValue("@answer", "0");
                command.Parameters.AddWithValue("@hint", "Используйте эффективный алгоритм факторизации для поиска чисел с 5 делителями");
                command.ExecuteNonQuery();

                // Заполняем задачу 7 (программирование - число Фибоначчи)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 8);
                command.Parameters.AddWithValue("@level_id", 7);
                command.Parameters.AddWithValue("@name", "ОЛД: *тревожные звуки системы* Последний шанс! Нам нужно найти число Фибоначчи, чтобы отключить систему самоликвидации лаборатории. Это критически важно для нашего побега. Поможешь мне решить эту программистскую задачу до окончания обратного отсчета?");
                command.Parameters.AddWithValue("@description", "ОЛД: *тревожные звуки системы* Последний шанс! Нам нужно найти число Фибоначчи, чтобы отключить систему самоликвидации лаборатории. Это критически важно для нашего побега.\n\nСистема самоликвидации использует специальный алгоритм, основанный на последовательности Фибоначчи. Мне нужно определить, каким по счету числом Фибоначчи является число 10946.\n\nНапомню, что последовательность Фибоначчи начинается так:\nF(1) = 1, F(2) = 1, F(3) = 2, F(4) = 3, F(5) = 5 и так далее.\n\nПомоги мне решить эту программистскую задачу до окончания обратного отсчета! Помни, что рекурсивный подход будет слишком медленным для больших чисел — используй итерацию.\n\nВведи номер числа Фибоначчи, равного 10946.");
                command.Parameters.AddWithValue("@answer", "0");
                command.Parameters.AddWithValue("@hint", "Используйте итерацию вместо рекурсии для вычисления чисел Фибоначчи");
                command.ExecuteNonQuery();

                // Заполняем задачу 8 (геометрия - соотношение планет)
                command.CommandText = "INSERT INTO task (id, level_id, name, description, answer, hint) " +
                                      "VALUES (@id, @level_id, @name, @description, @answer, @hint)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", 9);
                command.Parameters.AddWithValue("@level_id", 8);
                command.Parameters.AddWithValue("@name", "ОЛД: *звуки космического сканера* Срочно! Я получил сообщение с космической станции. Для нашего побега нам нужно использовать телепортационную установку, но она требует калибровки по планетарным данным. Поможешь решить эту геометрическую задачу, чтобы мы не оказались в черной дыре?");
                command.Parameters.AddWithValue("@description", "ОЛД: *звуки космического сканера* Эй, я получил срочное сообщение с космической станции! Для нашего побега нам нужно использовать телепортационную установку, но она требует калибровки по планетарным данным.\n\nСистема сообщает, что объем одной планеты в 1331 раз больше объема другой. Мне нужно знать, во сколько раз площадь поверхности первой планеты больше площади поверхности второй, чтобы правильно настроить телепорт.\n\nПомни, что если я введу неправильные данные, мы можем оказаться в черной дыре вместо безопасной зоны! Это геометрическая задача о соотношении объемов и площадей поверхностей сфер. Используй формулы объема и площади поверхности шара.\n\nБудь осторожен — от этого расчета зависит, сможем ли мы успешно телепортироваться и завершить наш побег из лаборатории!");
                command.Parameters.AddWithValue("@answer", "121");
                command.Parameters.AddWithValue("@hint", "Используйте соотношение радиусов для нахождения соотношения площадей");
                command.ExecuteNonQuery();

            }
        }

        // Метод отображения и обработки главного меню
        // Содержит основные опции, с которых начинается взаимодействие игрока с игрой
        static void ShowMainMenu()
        {
            // Бесконечный цикл для постоянного отображения меню до выбора валидной опции
            while (true)
            {
                Console.WriteLine("\n--- Главное меню ---");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Загрузить сохранение");
                Console.WriteLine("3. Удалить сохранение");
                Console.WriteLine("4. Настройки");
                Console.WriteLine("Обращаем внимание, уровни на программировнаие пока недоступны");
                Console.Write("Выберите опцию (1-4): ");
                string choice = Console.ReadLine();
                HashSet<long> mySet = new HashSet<long>();
                if (choice == "1")
                {
                    mySet.Clear();
                    command.CommandText = "SELECT * FROM session WHERE last_date IS NULL;";
                    DataTable data = new DataTable();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(data);
                    if (data.Rows.Count > 0)
                    {
                        Console.WriteLine("Выберите свободную сессию");
                        mySet.Clear();
                        foreach (DataRow row in data.Rows)
                        {
                            Console.WriteLine($"Сессия: {row.Field<long>("id")}");
                            mySet.Add(row.Field<long>("id"));
                        }
                        do
                        {
                            sessionId = Convert.ToInt64(Console.ReadLine());
                        }
                        while (!mySet.Contains(sessionId));
                        command.CommandText = "UPDATE session SET last_date = datetime('now') WHERE id = " + sessionId + ";";
                        command.ExecuteNonQuery();
                        Console.WriteLine("Выбрана сессия " + sessionId);
                        StartNewGame();
                        return; // Выход из метода после запуска игры
                    }
                    else
                    {
                        Console.WriteLine("Нет свободных сессий");
                    }
                }
                else if (choice == "2")
                {
                    mySet.Clear();
                    command.CommandText = "SELECT * FROM session WHERE last_date IS NOT NULL";
                    DataTable data = new DataTable();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(data);
                    if (data.Rows.Count > 0)
                    {
                        Console.WriteLine("Выберите сохраненную сессию:");
                        foreach (DataRow row in data.Rows)
                        {
                            Console.WriteLine($"Сессия: {row.Field<long>("id")}. Время: {row.Field<string>("last_date")}");
                            mySet.Add(row.Field<long>("id"));
                        }
                        do
                        {
                            sessionId = Convert.ToInt64(Console.ReadLine());
                        }
                        while (!mySet.Contains(sessionId));
                        command.CommandText =
                             "UPDATE session SET last_date = datetime('now') WHERE id = " + sessionId + ";";
                        command.ExecuteNonQuery();
                        Console.WriteLine("Выбрана сессия " + sessionId);
                        // запустить игру
                        Game.CurrentLevel = GetCurrentLevelBySessionID(sessionId) + 1;
                        Console.WriteLine("Игра продолжена с уровня: " + Game.CurrentLevel);
                        // Переход к основному циклу уровней
                        StartLevel();
                    }
                    else
                    {
                        Console.WriteLine("Нет сохраненных сессий");
                    }
                }
                else if (choice == "3")
                {
                    mySet.Clear();
                    command.CommandText = "SELECT * FROM session WHERE last_date IS NOT NULL";
                    DataTable data = new DataTable();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(data);
                    if (data.Rows.Count > 0)
                    {
                        Console.WriteLine("Выберите очищаемую сессию:");
                        foreach (DataRow row in data.Rows)
                        {
                            Console.WriteLine($"Сессия: {row.Field<long>("id")}. Время: {row.Field<string>("last_date")}");
                            mySet.Add(row.Field<long>("id"));
                        }
                        do
                        {
                            sessionId = Convert.ToInt64(Console.ReadLine());
                        }
                        while (!mySet.Contains(sessionId));
                        command.CommandText =
                            "UPDATE session SET last_date = NULL WHERE id = " + sessionId + "; " +
                            "DELETE FROM egg_progress WHERE session_id = " + sessionId + "; " +
                            "DELETE FROM task_progress WHERE session_id = " + sessionId + "; ";
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Нет сессий для очистки");
                    }
                }
                else if (choice == "4")
                {
                    // Переход в меню настроек
                    ShowSettings();
                }
                else
                {
                    // Обработка некорректного ввода
                    Console.WriteLine("Неверный ввод");
                }
            }
        }
        // Метод отображения и обработки меню настроек
        // Позволяет игроку настроить аспекты игры по своему вкусу
        static void ShowSettings()
        {
            // Бесконечный цикл для постоянного отображения меню настроек
            while (true)
            {
                Console.WriteLine("\n--- Настройки ---");
                // Динамическое отображение текущих настроек в пунктах меню
                Console.WriteLine($"1. Изменить музыку (текущая: {Game.Music})");
                Console.WriteLine($"2. Изменить текстуры (текущие: {Game.Textures})");
                // Динамическое изменение текста пункта меню в зависимости от текущего состояния
                Console.WriteLine($"3. {(Game.DeveloperMode ? "Деактивировать" : "Активировать")} режим разработчика");
                Console.WriteLine("4. Вернуться в главное меню");
                Console.Write("Выберите опцию (1-4): ");
                string choice = Console.ReadLine();
                // Обработка выбора пользователя
                switch (choice)
                {
                    case "1":
                        ChangeMusic(); // Переход к изменению музыки
                        break;
                    case "2":
                        ChangeTextures(); // Переход к изменению текстур
                        break;
                    case "3":
                        ToggleDeveloperMode(); // Переключение режима разработчика
                        break;
                    case "4":
                        return; // Возврат в главное меню
                    default:
                        Console.WriteLine("Неверный ввод");
                        break;
                }
            }
        }
        // Метод изменения музыкального сопровождения
        // Позволяет игроку выбрать один из 5 доступных вариантов музыки
        static void ChangeMusic()
        {
            Console.WriteLine("Выберите музыку (1-5):");
            string choice = Console.ReadLine();
            // Проверка, что ввод является числом в диапазоне 1-5
            if (int.TryParse(choice, out int musicNum) && musicNum >= 1 && musicNum <= 5)
            {
                // Установка выбранной музыки с форматированием названия
                Game.Music = $"Музыка {musicNum}";
                Console.WriteLine($"Музыка изменена на {Game.Music}");
            }
            else
            {
                Console.WriteLine("Неверный выбор");
            }
        }
        // Метод изменения текстур (визуального стиля)
        // Позволяет игроку выбрать один из 5 доступных визуальных стилей
        static void ChangeTextures()
        {
            Console.WriteLine("Выберите текстуры (1-5):");
            string choice = Console.ReadLine();
            // Проверка, что ввод является числом в диапазоне 1-5
            if (int.TryParse(choice, out int textureNum) && textureNum >= 1 && textureNum <= 5)
            {
                // Установка выбранных текстур с форматированием названия
                Game.Textures = $"Текстуры {textureNum}";
                Console.WriteLine($"Текстуры изменены на {Game.Textures}");
            }
            else
            {
                Console.WriteLine("Неверный выбор");
            }
        }
        // Метод переключения режима разработчика
        // Режим разработчика предоставляет дополнительные команды для отладки
        static void ToggleDeveloperMode()
        {
            // Инвертирование текущего состояния режима
            Game.DeveloperMode = !Game.DeveloperMode;
            // Вывод сообщения о текущем состоянии режима
            Console.WriteLine($"{(Game.DeveloperMode ? "Активирован" : "Деактивирован")} режим разработчика");
        }
        // Метод запуска новой игры
        // Сбрасывает все игровые данные и начинает игру с первого уровня
        static void StartNewGame()
        {
            // Установка текущего уровня на начальный (0)
            Game.CurrentLevel = 0;
            Console.WriteLine("Игра начата!");
            // Переход к основному циклу уровней
            StartLevel();
        }
        // Основной метод игрового процесса - управляет прохождением уровней
        // Содержит логику взаимодействия игрока с текущим уровнем
        static void StartLevel()
        {
            // Цикл продолжается, пока текущий уровень меньше 9 (всего 9 уровней: 0-8)
            while (Game.CurrentLevel < 9)
            {
                int currentLevel = Game.CurrentLevel;

                // Проверка, пройден ли уже текущий уровень
                if (IsLevelComplete(currentLevel, sessionId))
                {
                    // Переход к следующему уровню без отображения текущего
                    Game.CurrentLevel++;
                    continue;
                }
                // Отображение информации о текущем уровне
                Console.WriteLine($"\n--- Уровень {Game.CurrentLevel} ---");
                // Отображение приветственного сообщения от персонажа ОЛД
                Console.WriteLine(GetGreetingByTaskId(Game.CurrentLevel));
                // Предложение игроку начать решение задачи
                Console.WriteLine("1. Да");
                Console.WriteLine("2. Нет");
                Console.Write("Выберите (1 или 2): ");
                string answer = Console.ReadLine();
                // Обработка выбора игрока
                if (answer == "1")
                {
                    // Получение типа задачи для текущего уровня
                    string taskType = GetTaskTypeByTaskId(Game.CurrentLevel);
                    Console.WriteLine($"\n--- Задача {taskType} ---");
                    // Отображение условия задачи
                    Console.WriteLine(GetConditionByTaskId(Game.CurrentLevel));
                    // Переход к обработке ввода ответа
                    ProcessTask();
                }
                else if (answer == "2")
                {
                    // Режим свободного перемещения по уровню без решения задачи
                    Console.WriteLine("Вы свободны на уровне. Доступные команды: exit, question, hint, stat, egg");
                    // Если активирован режим разработчика, отображаются дополнительные команды
                    if (Game.DeveloperMode)
                    {
                        Console.WriteLine("Режим разработчика: levelX — переход на уровень X");
                    }
                    // Бесконечный цикл для обработки команд в режиме свободного перемещения
                    while (true)
                    {
                        string command = Console.ReadLine().ToLower();
                        // Обработка команды выхода в главное меню
                        if (command == "exit")
                        {
                            ShowMainMenu();
                            return;
                        }
                        // Обработка команды повторного отображения вопроса
                        else if (command == "question")
                        {
                            Console.WriteLine(GetGreetingByTaskId(Game.CurrentLevel));
                        }
                        // Обработка команды запроса подсказки
                        else if (command == "hint")
                        {
                            Console.WriteLine(GetHintByTaskId(Game.CurrentLevel));
                        }
                        // Обработка команды отображения статистики
                        else if (command == "stat")
                        {
                            ShowStatistics();
                        }
                        // Обработка команды перехода на следующий уровень (для разработчиков)
                        else if (command == "next")
                        {
                            Game.CurrentLevel++;
                            break;
                        }
                        // Обработка команды перехода на конкретный уровень (только в режиме разработчика)
                        else if (Game.DeveloperMode && command.StartsWith("level") && command.Length > 5)
                        {
                            string levelStr = command.Substring(5);
                            // Проверка, что после "level" следует число от 0 до 8
                            if (int.TryParse(levelStr, out int level) && level >= 0 && level <= 8)
                            {
                                Game.CurrentLevel = level;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Неверный номер уровня. Используйте формат: levelX, где X от 0 до 8");
                            }
                        }
                        // Обработка команды поиска пасхалки
                        else if (command == "egg")
                        {
                            string eggMessage = GetEggByLevelID(currentLevel);
                            if (!string.IsNullOrEmpty(eggMessage))
                            {
                                long eggId = GetEggIDByLevelID(currentLevel);
                                if (IsEggProgressWithSessionIDExists(eggId, sessionId))
                                {
                                    Console.WriteLine("Пасхалка уже собрана.");
                                }
                                else
                                {
                                    Console.WriteLine(eggMessage);
                                    CollectEgg(eggId, sessionId);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Здесь нет пасхалки");
                            }
                            continue;
                        }
                        else
                        {
                            // Обработка некорректной команды
                            Console.WriteLine("Неверная команда. Доступные команды: exit, question, advise, stat, egg");
                            if (Game.DeveloperMode)
                            {
                                Console.WriteLine("В режиме разработчика: levelX для перехода на уровень X");
                            }
                        }
                    }
                }
                else
                {
                    // Обработка некорректного выбора (не 1 и не 2)
                    Console.WriteLine("Введите 1 или 2.");
                }
            }
            // Достижение финального уровня (9)
            Console.WriteLine("\n--- Игра завершена! ---");
            Console.WriteLine("Вы достигли финального уровня!");
            Console.WriteLine("Примечание: уровень босса не реализован в бете");
            // Возврат в главное меню
            ShowMainMenu();
        }

        static void ProcessTask()
        {
            int currentLevel = Game.CurrentLevel;

            Console.WriteLine("Доступные команды: hint - подсказка, question - повторить приветствие, stat - статистика, egg - поиск пасхалки, exit - выход");
            Console.WriteLine("Введите ответ числом:");

            // Бесконечный цикл для обработки ввода
            while (true)
            {
                string input = Console.ReadLine().ToLower();

                // 1. Сначала обрабатываем все возможные команды
                // Обработка команды запроса подсказки
                if (input == "hint")
                {
                    Console.WriteLine(GetHintByTaskId(currentLevel));
                    continue;
                }
                // Обработка команды повторного отображения вопроса
                else if (input == "question")
                {
                    Console.WriteLine(GetGreetingByTaskId(currentLevel));
                    continue;
                }
                // Обработка команды отображения статистики
                else if (input == "stat")
                {
                    ShowStatistics();
                    continue;
                }
                // Обработка команды поиска пасхалки
                else if (input == "egg")
                {
                    string eggMessage = GetEggByLevelID(currentLevel);
                    if (!string.IsNullOrEmpty(eggMessage))
                    {   
                        long eggId = GetEggIDByLevelID(currentLevel);
                        if (IsEggProgressWithSessionIDExists(eggId, sessionId))
                        {
                            Console.WriteLine("Пасхалка уже собрана.");
                        }
                        else
                        {
                            Console.WriteLine(eggMessage);
                            CollectEgg(eggId, sessionId);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Здесь нет пасхалки");
                    }
                    continue;
                }
                // Обработка команды выхода
                else if (input == "exit")
                {
                    ShowMainMenu();
                    return;
                }

                // 2. Если ввод не является командой, проверяем, является ли он числом
                else if (double.TryParse(input, out double userAnswer))
                {
                    double correctAnswer = GetCorrectAnswerByLevelID(currentLevel);

                    // Проверка правильности ответа с допустимой погрешностью 0.1
                    if (Math.Abs(userAnswer - correctAnswer) < 0.1)
                    {
                        Console.WriteLine("Правильный ответ!");
                        // Отметка уровня как пройденного
                        CompleteLevel(currentLevel, sessionId);
                        // Переход к меню после правильного ответа
                        HandlePostTask();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Неправильный ответ. Попробуйте еще раз или введите команду:");
                        Console.WriteLine("hint - для подсказки, question - повторить приветствие, stat - статистика, egg - поиск пасхалки, exit - для выхода");
                    }
                }
                // 3. Если ввод не является ни командой, ни числом
                else
                {
                    Console.WriteLine("Неизвестная команда. Доступные команды: hint, question, stat, egg, exit");
                    Console.WriteLine("Или введите числовой ответ:");
                }
            }
        }
        // Метод отображения меню после правильного ответа
        // Предоставляет игроку выбор дальнейших действий
        static void HandlePostTask()
        {
            Console.WriteLine("Доступные команды: exit, next, stat, egg");
            while (true)
            {
                string command = Console.ReadLine().ToLower();
                // Обработка команды выхода в главное меню
                if (command == "exit")
                {
                    ShowMainMenu();
                    return;
                }
                // Обработка команды перехода на следующий уровень
                else if (command == "next")
                {
                    Game.CurrentLevel++;
                    return;
                }
                // Обработка команды отображения статистики
                else if (command == "stat")
                {
                    ShowStatistics();
                }
                // Обработка команды поиска пасхалки
                else if (command == "egg")
                {
                    string eggMessage = GetEggByLevelID(Game.CurrentLevel);
                    if (!string.IsNullOrEmpty(eggMessage))
                    {
                        long eggId = GetEggIDByLevelID(Game.CurrentLevel);
                        if (IsEggProgressWithSessionIDExists(eggId, sessionId))
                        {
                            Console.WriteLine("Пасхалка уже собрана.");
                        }
                        else
                        {
                            Console.WriteLine(eggMessage);
                            CollectEgg(eggId, sessionId);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Здесь нет пасхалки");
                    }
                }
                else
                {
                    Console.WriteLine("Неверная команда. Доступные команды: exit, next, stat, egg");
                }
            }
        }
        // Метод отображения статистики игрока
        // Собирает и отображает текущую информацию о прогрессе игры
        static void ShowStatistics()
        {
            // Подсчет пройденных уровней
            int completedLevels = 0;
            for (long i = 0; i < 9; i++)
            {
                if (IsLevelComplete(i, sessionId)) completedLevels++;
            }
            // Форматированный вывод статистической информации
            Console.WriteLine("\n--- Статистика игрока ---");
            Console.WriteLine($"Пройдено уровней: {completedLevels}/9");
            Console.WriteLine($"Собрано пасхалок: {GetEggCountBySessionID(sessionId)}/4");
            Console.WriteLine($"Музыка: {Game.Music}");
            Console.WriteLine($"Текстуры: {Game.Textures}");
            Console.WriteLine($"Режим разработчика: {(Game.DeveloperMode ? "Активирован" : "Деактивирован")}");
            Console.WriteLine("Введите команду: exit, next, hint, stat, egg");
        }
    }
}