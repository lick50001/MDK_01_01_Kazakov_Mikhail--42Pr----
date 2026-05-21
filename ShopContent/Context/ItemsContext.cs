using System;
using System.Collections.ObjectModel;
using System.Linq;
using MySql.Data.MySqlClient;
using ShopContent.Classes;
using ShopContent.Modell;

namespace ShopContent.Context
{
    public class ItemsContext : Modell.Items
    {
        public ItemsContext(bool save = false)
        {
            if (save) Save(true);
            Category = new Categorys();
        }

        public static ObservableCollection<ItemsContext> AllItems()
        {
            var allItems = new ObservableCollection<ItemsContext>();
            // Загружаем категории ОДИН РАЗ
            var allCategorys = CategorysContext.AllCategorys();

            MySqlConnection conn;
            MySqlDataReader dataItems = Connection.Query("SELECT * FROM Items", out conn);

            while (dataItems.Read())
            {
                int? categoryId = dataItems.IsDBNull(4) ? (int?)null : dataItems.GetInt32(4);
                var category = categoryId.HasValue
                    ? allCategorys.FirstOrDefault(x => x.Id == categoryId.Value)
                    : null;

                allItems.Add(new ItemsContext()
                {
                    Id = dataItems.GetInt32(0),
                    Name = dataItems.GetString(1),
                    Price = dataItems.GetDouble(2),
                    Description = dataItems.IsDBNull(3) ? "" : dataItems.GetString(3),
                    Category = category
                });
            }
            Connection.CloseConnection(conn);
            return allItems;
        }

        public void Save(bool isNew = false)
        {
            using (var conn = Connection.OpenConnection())
            {
                if (isNew)
                {
                    // Безопасный INSERT с параметрами + получение LAST_INSERT_ID()
                    string sql = "INSERT INTO Items (Name, Price, Description, IdCategory) VALUES (@name, @price, @desc, @catId); SELECT LAST_INSERT_ID();";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", Name ?? "");
                        cmd.Parameters.AddWithValue("@price", Price);
                        cmd.Parameters.AddWithValue("@desc", Description ?? "");
                        cmd.Parameters.AddWithValue("@catId", Category?.Id ?? (object)DBNull.Value);

                        object result = cmd.ExecuteScalar();
                        this.Id = Convert.ToInt32(result);
                    }
                }
                else
                {
                    string sql = "UPDATE Items SET Name = @name, Price = @price, Description = @desc, IdCategory = @catId WHERE Id = @id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", Name ?? "");
                        cmd.Parameters.AddWithValue("@price", Price);
                        cmd.Parameters.AddWithValue("@desc", Description ?? "");
                        cmd.Parameters.AddWithValue("@catId", Category?.Id ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", Id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MainWindow.init.frame.Navigate(MainWindow.init.Main);
        }

        public void Delete()
        {
            using (var conn = Connection.OpenConnection())
            {
                string sql = "DELETE FROM Items WHERE Id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public RelayCommand OnEdit => new RelayCommand(obj =>
        {
            MainWindow.init.frame.Navigate(new View.Add(this));
        });

        public RelayCommand OnSave => new RelayCommand(obj =>
        {
            // Обновляем категорию из актуального списка
            var allCats = CategorysContext.AllCategorys();
            Category = allCats.FirstOrDefault(x => x.Id == Category?.Id);
            Save(false); // isNew = false
        });

        public RelayCommand OnDelete => new RelayCommand(obj =>
        {
            Delete();
            (MainWindow.init.Main.DataContext as ViewModell.VMItems)?.Items.Remove(this);
        });
    }
}