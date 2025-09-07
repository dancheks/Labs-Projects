using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace SqlliteViewer.Views
{
    public partial class StoragesView : UserControl
    {
        private int? selectedId = null;

        public StoragesView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Storages", conn);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            StoragesGrid.ItemsSource = dt.DefaultView;
            StoragesGrid.SelectedIndex = -1;
            selectedId = null;
        }

        private void ClearInputs()
        {
            CodeNameBox.Text = "";
            AddressBox.Text = "";
            PalletsHoldsBox.Text = "";
        }

        private void StoragesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StoragesGrid.SelectedItem is DataRowView row)
            {
                selectedId = Convert.ToInt32(row["id"]);
                CodeNameBox.Text = row["Code_Name"].ToString();
                AddressBox.Text = row["Address"].ToString();
                PalletsHoldsBox.Text = row["Pallets_Holds"].ToString();
            }
            else
            {
                selectedId = null;
                ClearInputs();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(@"
                INSERT INTO Storages (Code_Name, Address, Pallets_Holds)
                VALUES (@c, @a, @p)", conn);

            cmd.Parameters.AddWithValue("@c", CodeNameBox.Text);
            cmd.Parameters.AddWithValue("@a", AddressBox.Text);
            cmd.Parameters.AddWithValue("@p", PalletsHoldsBox.Text);
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
                UPDATE Storages
                SET Code_Name=@c, Address=@a, Pallets_Holds=@p
                WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.Parameters.AddWithValue("@c", CodeNameBox.Text);
            cmd.Parameters.AddWithValue("@a", AddressBox.Text);
            cmd.Parameters.AddWithValue("@p", PalletsHoldsBox.Text);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM Storages WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }
    }
}