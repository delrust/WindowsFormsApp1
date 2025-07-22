using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;

namespace WindowsFormsApp1
{
    public class dataBaseManager
    {
        string DBname;
        string connectionString;
        string serverName = "Ваш сервер";
        string MainTable = "MainBase";

        public List<string> GetDataBases()
        {
            List<string> list = new List<string>();
            string connectionString = $"Data Source={serverName};Integrated Security=True";
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


                            list.Add((string)reader["name"]);
                        }
                    }
                }
            }
            return list;
        }

        public void ConnectToDB(string DBname)
        {
            this.DBname = DBname;
            connectionString = $"Server=DESKTOP-5L65RAI;Database={DBname};Integrated Security=True;";

            CheckAndCreateTablesIfNeed();
        }

        public bool CreateNewDB(string DBname)
        {
            string connectionString = @"Data Source=DESKTOP-5L65RAI;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем, существует ли уже база данных
                string checkQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{DBname}'";

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    int exists = (int)checkCommand.ExecuteScalar();

                    if (exists == 0)
                    {
                        string createQuery = $"CREATE DATABASE {DBname}";
                        using (SqlCommand createCommand = new SqlCommand(createQuery, connection))
                        {
                            createCommand.ExecuteNonQuery();
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void CheckAndCreateTablesIfNeed()
        {
            string TableName = "MainBase";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (!TableExists(TableName, connection))
                    {
                        string createTableQuery = $@"CREATE TABLE {TableName} (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        name NVARCHAR(100) NOT NULL UNIQUE,
                        content NVARCHAR(MAX) CHECK (ISJSON(content) = 1),
                        isRoot BIT NOT NULL
                        )";

                        using (SqlCommand command = new SqlCommand(createTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                            MessageBox.Show("Таблица успешно создана!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании таблицы: {ex.Message}");
            }
        }
        private bool TableExists(string tableName, SqlConnection connection)
        {
            string query = @" SELECT CASE WHEN EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName) 
            THEN 1 ELSE 0 END";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);
                return (int)command.ExecuteScalar() == 1;
            }
        }



        public int CreateNewComponent(string name, string parentName = "", bool isRoot = true, int count = 0)
        {
            int result = 0;
            string jsonContent = "[]";
            string insertQuery = string.Empty;
            int isroot = isRoot ? 1 : 0;

            insertQuery = $"INSERT INTO {MainTable} (name, content, isRoot) OUTPUT INSERTED.Id VALUES ('" + name + "', '[]', " + isroot + ")";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Добавляем параметры для защиты от SQL-инъекций
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@content", string.IsNullOrEmpty(jsonContent) ? (object)DBNull.Value : jsonContent);
                        command.Parameters.AddWithValue("@isRoot", isRoot);

                        result = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
            }
            return result;
        }



        /*
        public void AddComponentToObj(string name, string parentName, int count)
        {
            int id = CreateNewComponent(name, parentName, false, count);

            string GetQuery = $"SELECT content From {MainTable} WHERE name = '{parentName}'";
            string result;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(GetQuery, connection))
                {
                    result = command.ExecuteScalar()?.ToString();
                }
            }
            List<MyData> fd = null;
            if (result != null)
                fd = JsonSerializer.Deserialize<List<MyData>>(result);

            MyData obj = new MyData { id = id, count = count };

            if (fd != null)
                fd.Add(obj);

            string jsonContent = JsonSerializer.Serialize(fd);

            string UpdateQuery = $"UPDATE {MainTable} SET content = '{jsonContent}' WHERE name = '{parentName}'";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(UpdateQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
            }
        }

        public List<Component> GetAllComponents()
        {
            string query = $"SELECT * FROM {MainTable} WHERE isRoot = 1";
            List<Component> components = new List<Component>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var component = new Component
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                JsonChildren = reader.GetString(reader.GetOrdinal("content"))
                            };
                            component.Init();
                            components.Add(component);

                            // Загружаем вложенные компоненты
                            LoadChildComponents(component);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении корневых компонентов: {ex.Message}");
            }

            return components;
        }

        */

        public void AddComponentToObj(string name, string parentName, int count)
        {
            // Сначала проверим, не пытаемся ли мы добавить компонент сам в себя
            if (name == parentName)
            {
                Console.WriteLine("Ошибка: нельзя добавить компонент сам в себя");
                MessageBox.Show("Не допускается рекурсивная вложенность");
                return;
            }

            // Проверим, не создаем ли мы циклическую зависимость
            if (WouldCreateCycle(name, parentName))
            {
                Console.WriteLine("Ошибка: добавление этого компонента создаст циклическую зависимость");
                MessageBox.Show("Не допускается рекурсивная вложенность");
                return;
            }

            int id = CreateNewComponent(name, parentName, false, count);

            string GetQuery = $"SELECT content From {MainTable} WHERE name = '{parentName}'";
            string result;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(GetQuery, connection))
                {
                    result = command.ExecuteScalar()?.ToString();
                }
            }
            List<MyData> fd = null;
            if (result != null)
                fd = JsonSerializer.Deserialize<List<MyData>>(result);

            MyData obj = new MyData { id = id, count = count };

            if (fd != null)
                fd.Add(obj);

            string jsonContent = JsonSerializer.Serialize(fd);

            string UpdateQuery = $"UPDATE {MainTable} SET content = '{jsonContent}' WHERE name = '{parentName}'";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(UpdateQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
            }
        }

        // Метод для проверки на циклические зависимости
        private bool WouldCreateCycle(string newComponentName, string parentName)
        {
            // Получаем все компоненты
            var allComponents = GetAllComponentsWithHierarchy();

            // Находим родительский компонент
            var parentComponent = allComponents.FirstOrDefault(c => c.Name == parentName);
            if (parentComponent == null) return false;

            // Проверяем всех родителей нового компонента
            return IsChildOf(newComponentName, parentName, allComponents);
        }

        // Рекурсивная проверка, является ли potentialChild потомком potentialParent
        private bool IsChildOf(string potentialChild, string potentialParent, List<Component> allComponents)
        {
            var childComponent = allComponents.FirstOrDefault(c => c.Name == potentialChild);
            if (childComponent == null) return false;

            // Если у компонента нет детей, то он не может быть родителем
            if (string.IsNullOrEmpty(childComponent.JsonChildren)) return false;

            var children = JsonSerializer.Deserialize<List<MyData>>(childComponent.JsonChildren);
            foreach (var child in children)
            {
                var childName = allComponents.FirstOrDefault(c => c.Id == child.id)?.Name;
                if (childName == potentialParent) return true;
                if (IsChildOf(childName, potentialParent, allComponents)) return true;
            }

            return false;
        }

        // Метод для получения всех компонентов с их иерархией
        private List<Component> GetAllComponentsWithHierarchy()
        {
            string query = $"SELECT * FROM {MainTable}";
            List<Component> components = new List<Component>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var component = new Component
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                JsonChildren = reader.GetString(reader.GetOrdinal("content")),
                                IsRoot = reader.GetBoolean(reader.GetOrdinal("isRoot")) == true ? 1 : 0
                            };
                            components.Add(component);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении компонентов: {ex.Message}");
            }

            return components;
        }

        public List<Component> GetAllComponents()
        {
            string query = $"SELECT * FROM {MainTable} WHERE isRoot = 1";
            List<Component> components = new List<Component>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var component = new Component
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                JsonChildren = reader.GetString(reader.GetOrdinal("content")),
                                IsRoot = reader.GetBoolean(reader.GetOrdinal("isRoot")) == true ? 1 : 0
                            };
                            component.Init();
                            components.Add(component);

                            // Загружаем вложенные компоненты
                            LoadChildComponents(component);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении корневых компонентов: {ex.Message}");
            }

            return components;
        }







        private void LoadChildComponents(Component parentComponent)
        {
            if (parentComponent?.Children == null || parentComponent.Children.Count == 0)
                return;

            foreach (var child in parentComponent.Children)
            {
                try
                {
                    string query = $"SELECT * FROM {MainTable} WHERE id = {child.Id}";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(query, connection);
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                child.Name = reader.GetString(reader.GetOrdinal("name"));
                                child.JsonChildren = reader.GetString(reader.GetOrdinal("content"));
                                child.Init();

                                // Рекурсивно загружаем дочерние элементы
                                LoadChildComponents(child);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке компонента {child.Id}: {ex.Message}");
                }
            }
        }


        public string GetComponent(int id)
        {
            string query = $"SELECT * FROM {MainTable} WHERE id = {id}";

            string result = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader.GetString(reader.GetOrdinal("name"));
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
            }


            return result;
        }



        public void DeleteComponents(string componentName, string parentName)
        {
            string getQouery = $"SELECT * FROM {MainTable} WHERE name = '{componentName}'";
            string result = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(getQouery, connection);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isRoot = reader.GetBoolean(reader.GetOrdinal("isRoot"));
                            result = isRoot ? "1" : "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
            }


            if (result == "1")
            {
                string query = "DELETE FROM table WHERE name = @Name";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Параметризованный запрос для защиты от SQL-инъекций
                            command.Parameters.AddWithValue("@Name", componentName);

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();
                            Console.WriteLine($"Удалено строк: {rowsAffected}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении: {ex.Message}");
                }
            }
            else if (result == "0")
            {
                string GetQuery = $"SELECT content From {MainTable} WHERE name = '{parentName}'";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(GetQuery, connection))
                    {
                        //command.Parameters.AddWithValue("@Name", "пример");
                        result = command.ExecuteScalar()?.ToString();

                    }

                }
                List<MyData> fd = null;
                if (result != null)
                    fd = JsonSerializer.Deserialize<List<MyData>>(result);

                if (fd != null)
                {
                    int delId = 0;
                    foreach (MyData data in fd)
                    {
                        if (GetComponent(data.id) == componentName)
                        {
                            delId = data.id;


                        }
                    }

                    fd = fd.Where(x => x.id != delId).ToList();
                }


                string jsonContent = JsonSerializer.Serialize(fd);

                string UpdateQuery = $"UPDATE {MainTable} SET content = '{jsonContent}' WHERE name = '{parentName}'";
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(UpdateQuery, connection))
                        {
                            command.ExecuteNonQuery();

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
                }
            }
        }

        public bool Update(string oldName, string newName)
        {
            string query = $"UPDATE {MainTable} SET name = '{newName}' WHERE name = '{oldName}'";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Обновлено строк: {rowsAffected}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении: {ex.Message}");
                return false;
            }
        }
    }
    class MyData
    {
        public int id { get; set; }
        public int count { get; set; }
    }
}
