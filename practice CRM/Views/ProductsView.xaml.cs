using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SqlliteViewer.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductsView.xaml
    /// </summary>
    public partial class ProductsView : UserControl
    {
        private int? selectedId = null;

        public ProductsView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Products", conn);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            ProductsGrid.ItemsSource = dt.DefaultView;
            ProductsGrid.SelectedIndex = -1;
            selectedId = null;
        }

        private void ClearInputs()
        {
            NameBox.Text = WeightBox.Text = SpeedBox.Text =
            StressBox.Text = LengthBox.Text = WidthBox.Text = NumberInPalletBox.Text = "";
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is DataRowView row)
            {
                selectedId = Convert.ToInt32(row["id"]);
                NameBox.Text = row["Name"].ToString();
                WeightBox.Text = row["Weight"].ToString();
                SpeedBox.Text = row["Speed"].ToString();
                StressBox.Text = row["Stress"].ToString();
                LengthBox.Text = row["Length"].ToString();
                WidthBox.Text = row["Width"].ToString();
                NumberInPalletBox.Text = row["Number_In_Pallet"].ToString();
            }
            else
            {
                selectedId = null;
                NameBox.Text = "";
                WeightBox.Text = "";
                SpeedBox.Text = "";
                StressBox.Text = "";
                LengthBox.Text = "";
                WidthBox.Text = "";
                NumberInPalletBox.Text = "";
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(@"
            INSERT INTO Products (Name, Weight, Speed, Stress, Length, Width, Number_In_Pallet)
            VALUES (@n, @w, @s, @str, @l, @wd, @np)", conn);

            cmd.Parameters.AddWithValue("@n", NameBox.Text);
            cmd.Parameters.AddWithValue("@w", WeightBox.Text);
            cmd.Parameters.AddWithValue("@s", SpeedBox.Text);
            cmd.Parameters.AddWithValue("@str", StressBox.Text);
            cmd.Parameters.AddWithValue("@l", LengthBox.Text);
            cmd.Parameters.AddWithValue("@wd", WidthBox.Text);
            cmd.Parameters.AddWithValue("@np", NumberInPalletBox.Text);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(@"
            UPDATE Products
            SET Name=@n, Weight=@w, Speed=@s, Stress=@str, Length=@l, Width=@wd, Number_In_Pallet=@np
            WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.Parameters.AddWithValue("@n", NameBox.Text);
            cmd.Parameters.AddWithValue("@w", WeightBox.Text);
            cmd.Parameters.AddWithValue("@s", SpeedBox.Text);
            cmd.Parameters.AddWithValue("@str", StressBox.Text);
            cmd.Parameters.AddWithValue("@l", LengthBox.Text);
            cmd.Parameters.AddWithValue("@wd", WidthBox.Text);
            cmd.Parameters.AddWithValue("@np", NumberInPalletBox.Text);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM Products WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }
    }
}
