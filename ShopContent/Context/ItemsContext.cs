using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ObservableCollection<ItemsContext> allItems = new ObservableCollection<ItemsContext>();
            ObservableCollection<CategorysContext> allCategorys = new ObservableCollection<CategorysContext>();
            SqlConnection conn;
            SqlDataReader dataItems = Connection.Query("SELECT * FROM [dbo].[Items]", out conn);

            while (dataItems.Read())
            {
                allItems.Add(new ItemsContext()
                {
                    Id = dataItems.GetInt32(0),
                    Name = dataItems.GetString(1),
                    Price = dataItems.GetDouble(2),
                    Description = dataItems.GetString(3),
                    Category = dataItems.IsDBNull(4) ? null : allCategorys.Where(x => x.Id == dataItems.GetInt32(4)).First()
                });
            }
            Connection.CloseConnection(conn);
            return allItems;
        }
        
        public void Save(bool New = false)
        {
            SqlConnection conn;
            if (New)
            {
                SqlDataReader dataItems = Connection.Query("INSERT INTO" +
                    "[dbo].[Items](" +
                    "Name, " +
                    "Price " +
                    "Description)" +
                    "OUTPUT Inserted.Id " +
                    $"VALUES (" +
                    $"N'{this.Name}'," +
                    $"{this.Price}," +
                    $"N'{this.Description}')", out conn);
                dataItems.Read();
                this.Id = dataItems.GetInt32(0);
            }
            else
            {
                Connection.Query("UPDATE [dbo]. [ Items] " +
                    "SET " +
                    $"Name = N'{this.Name}', " +
                    $"Price = {this.Price}, " +
                    $"Description = N'{this.Description}', " +
                    $"IdCategory = {this.Category.Id} " +
                    $"WHERE " +
                    $"Id = {this.Id}", out conn);
            }
            Connection.CloseConnection(conn);
            MainWindow.init.frame.Navigate(MainWindow.init.Main);
        }

        public void Delete()
        {
            SqlConnection conn;
            Connection.Query("DELETE FROM [dbo].[Items]" +
                "WHERE" +
                $"Id = {this.Id}", out conn);
            Connection.CloseConnection(conn);
        }

        public RelayCommand OnEdit
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    MainWindow.init.frame.Navigate(new View.Add(this));
                });
            }
        }

        public RelayCommand OnSave
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Category = CategorysContext.AllCategorys().Where(x => x.Id == this.Category.Id).First();
                    Save();
                });
            }
        }

        public RelayCommand OnDelete
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Delete();
                    (MainWindow.init.Main.DataContext as ViewModell.VMItems).Items.Remove(this);
                });
            }
        }
    }
}
