using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SqlliteViewer.Views
{
    public partial class StoredView : UserControl
    {
        private int? selectedId = null;

        public StoredView()
        {
            InitializeComponent();
            LoadData();
            LoadForeignKeys();
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            string query = @"
        SELECT s.id,
               s.Storage AS StorageId,
               s.Product AS ProductId,
               st.Code_Name AS StorageName,
               p.Name AS ProductName,
               s.Pallets_Stored
        FROM Stored s
        LEFT JOIN Storages st ON s.Storage = st.id
        LEFT JOIN Products p ON s.Product = p.id";

            using var cmd = new SQLiteCommand(query, conn);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            StoredGrid.ItemsSource = dt.DefaultView;
            StoredGrid.SelectedIndex = -1;
            selectedId = null;
        }


        private void ClearInputs()
        {
            StorageBox.Text = "";
            ProductBox.Text = "";
            PalletsStoredBox.Text = "";
            FreeSpaceTextBlock.Text = "Свободно: —";
        }

        private void StoredGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StoredGrid.SelectedItem is DataRowView row)
            {
                selectedId = Convert.ToInt32(row["id"]);
                StorageBox.SelectedValue = row["StorageId"];
                ProductBox.SelectedValue = row["ProductId"];

                if (StorageBox.SelectedValue != null
            && int.TryParse(StorageBox.SelectedValue.ToString(), out var sid))
                {
                    ShowFreeSpace(sid);
                }

                PalletsStoredBox.Text = row["Pallets_Stored"].ToString();
            }
            else
            {
                selectedId = null;
                ClearInputs();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка обязательных полей
            if (!(StorageBox.SelectedValue != null
        && int.TryParse(StorageBox.SelectedValue.ToString(), out int storageId)) ||
                !(ProductBox.SelectedValue != null
        && int.TryParse(ProductBox.SelectedValue.ToString(), out int productId)) ||
                !int.TryParse(PalletsStoredBox.Text, out int newQty))
            {
                MessageBox.Show("Пожалуйста, выберите склад, продукт и введите корректное число паллет.",
                                "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            // 2. Ищем существующую запись
            using var checkCmd = new SQLiteCommand(
                "SELECT id, Pallets_Stored FROM Stored WHERE Storage=@s AND Product=@p", conn);
            checkCmd.Parameters.AddWithValue("@s", storageId);
            checkCmd.Parameters.AddWithValue("@p", productId);

            using var reader = checkCmd.ExecuteReader();
            if (reader.Read())
            {
                int existingId = reader.GetInt32(0);
                int existingQty = reader.GetInt32(1);
                reader.Close();

                // 3. Спрашиваем у пользователя, как поступить
                var result = MessageBox.Show(
                    $"На этом складе уже есть {existingQty} паллет товара.\n\n" +
                    "Нажмите 'Да' чтобы добавить к существующему количеству,\n" +
                    "'Нет' чтобы заменить на новое, или 'Отмена' чтобы ничего не делать.",
                    "Дублирующая запись",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) return;

                int finalQty = result == MessageBoxResult.Yes
                               ? existingQty + newQty  // складываем
                               : newQty;               // заменяем

                // 4. Обновляем запись
                using var updateCmd = new SQLiteCommand(
                    "UPDATE Stored SET Pallets_Stored=@ps WHERE id=@id", conn);
                updateCmd.Parameters.AddWithValue("@ps", finalQty);
                updateCmd.Parameters.AddWithValue("@id", existingId);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
                // 5. Если нет записи — вставляем новую
                using var insertCmd = new SQLiteCommand(@"
            INSERT INTO Stored (Storage, Product, Pallets_Stored)
            VALUES (@s, @p, @ps)", conn);
                insertCmd.Parameters.AddWithValue("@s", storageId);
                insertCmd.Parameters.AddWithValue("@p", productId);
                insertCmd.Parameters.AddWithValue("@ps", newQty);
                insertCmd.ExecuteNonQuery();
            }

            // 6. Обновляем грид и очищаем ввод
            LoadData();
            ClearInputs();
        }


        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(@"
                UPDATE Stored
                SET Storage=@s, Product=@p, Pallets_Stored=@ps
                WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.Parameters.AddWithValue("@s", StorageBox.SelectedValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p", ProductBox.SelectedValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ps", PalletsStoredBox.Text);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM Stored WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void LoadForeignKeys()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            // Загрузка складов
            using var storageCmd = new SQLiteCommand("SELECT id, Code_Name FROM Storages", conn);
            using var storageAdapter = new SQLiteDataAdapter(storageCmd);
            var storageTable = new DataTable();
            storageAdapter.Fill(storageTable);
            StorageBox.ItemsSource = storageTable.DefaultView;

            // Загрузка товаров
            using var productCmd = new SQLiteCommand("SELECT id, Name FROM Products", conn);
            using var productAdapter = new SQLiteDataAdapter(productCmd);
            var productTable = new DataTable();
            productAdapter.Fill(productTable);
            ProductBox.ItemsSource = productTable.DefaultView;
        }

        private void StorageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StorageBox.SelectedValue != null
        && int.TryParse(StorageBox.SelectedValue.ToString(), out int storageId))
            {
                ShowFreeSpace(storageId);
            }
            else
            {
                FreeSpaceTextBlock.Text = "Свободно: —";
            }
        }

        private void ShowFreeSpace(int storageId)
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            // Вместимость склада
            int capacity = 0;
            using (var capCmd = new SQLiteCommand(
                "SELECT Pallets_Holds FROM Storages WHERE id = @id", conn))
            {
                capCmd.Parameters.AddWithValue("@id", storageId);
                var result = capCmd.ExecuteScalar();
                int.TryParse(result?.ToString(), out capacity);
            }

            // Уже расставлено паллет на этом складе
            int occupied = 0;
            using (var occCmd = new SQLiteCommand(
                "SELECT IFNULL(SUM(Pallets_Stored), 0) FROM Stored WHERE Storage = @id", conn))
            {
                occCmd.Parameters.AddWithValue("@id", storageId);
                var result = occCmd.ExecuteScalar();
                int.TryParse(result?.ToString(), out occupied);
            }

            int freeSpace = capacity - occupied;
            FreeSpaceTextBlock.Text = $"Свободно: {freeSpace}";
            if (freeSpace == 0){
                FreeSpaceTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
                FreeSpaceTextBlock.Foreground = new SolidColorBrush(Colors.Lime);
        }
    }
}
