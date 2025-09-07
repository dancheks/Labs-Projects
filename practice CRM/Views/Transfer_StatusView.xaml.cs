using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SqlliteViewer.Views
{
    public partial class Transfer_StatusView : UserControl
    {
        private int? selectedId = null;
        private DataTable _allStorages;
        private DataTable _allProducts;
        private bool _isUpdating = false;
        private int? _lastStorageId = null;
        private int? _lastProductId = null;

        public Transfer_StatusView()
        {
            InitializeComponent();
            PalletsBox.PreviewTextInput += PalletsBox_PreviewTextInput;
            PalletsBox.TextChanged += PalletsBox_TextChanged;
            LoadData();
            LoadForeignKeys();
        }

        private void PalletsBox_PreviewTextInput(object s, TextCompositionEventArgs e)
        {
            // Только цифры
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void PalletsBox_TextChanged(object s, TextChangedEventArgs e)
        {
            if (!int.TryParse(PalletsBox.Text, out int v)) return;
            if (PalletsBox.Tag is int max && v > max)
            {
                PalletsBox.Text = max.ToString();
                PalletsBox.CaretIndex = PalletsBox.Text.Length;
            }
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            string query = @"
                SELECT ts.id,
               ts.Storage AS StorageId,
               st.Code_Name AS StorageName,
               ts.Product AS ProductId,
               p.Name AS ProductName,
               ts.Recipient AS RecipientId,
               r.Name AS RecipientName,
               ts.Number_Of_Pallets,
               CASE ts.Status
                   WHEN 1 THEN 'Подготовка'
                   WHEN 2 THEN 'Идёт'
                   WHEN 3 THEN 'Прибыл'
                   WHEN 4 THEN 'Задержан'
                   WHEN 5 THEN 'Отмена'
                   ELSE '—'
               END AS StatusText,
               ts.Status AS StatusCode,
               ts.Date_Of_Completion,
               ts.Date_Of_Issue,
               ts.Date_Of_Arrival
        FROM Transfer_Status ts
        LEFT JOIN Storages st ON ts.Storage = st.id
        LEFT JOIN Products p ON ts.Product = p.id
        LEFT JOIN Recipients r ON ts.Recipient = r.id";

            using var cmd = new SQLiteCommand(query, conn);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            TransferGrid.ItemsSource = dt.DefaultView;
            TransferGrid.SelectedIndex = -1;
            selectedId = null;
        }

        private void ClearInputs()
        {
            StorageBox.Text = "";
            ProductBox.Text = "";
            RecipientBox.Text = "";
            PalletsBox.Text = "";
            StatusBox.Text = "";
            CompletionDatePicker.SelectedDate = null;
            IssueDatePicker.SelectedDate = null;
            ArrivalDatePicker.SelectedDate = null;
        }

        private void TransferGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransferGrid.SelectedItem is DataRowView row)
            {
                selectedId = Convert.ToInt32(row["id"]);
                StorageBox.SelectedValue = row["StorageId"];
                ProductBox.SelectedValue = row["ProductId"];
                RecipientBox.SelectedValue = row["RecipientId"];
                PalletsBox.Text = row["Number_Of_Pallets"].ToString();
                if (int.TryParse(row["StatusText"]?.ToString(), out int status))
                {
                    foreach (ComboBoxItem item in StatusBox.Items)
                    {
                        if (item.Tag?.ToString() == status.ToString())
                        {
                            StatusBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                DateTime.TryParse(row["Date_Of_Completion"]?.ToString(), out var d1);
                DateTime.TryParse(row["Date_Of_Issue"]?.ToString(), out var d2);
                DateTime.TryParse(row["Date_Of_Arrival"]?.ToString(), out var d3);
                CompletionDatePicker.SelectedDate = d1 != DateTime.MinValue ? d1 : null;
                IssueDatePicker.SelectedDate = d2 != DateTime.MinValue ? d2 : null;
                ArrivalDatePicker.SelectedDate = d3 != DateTime.MinValue ? d3 : null;
            }
            else
            {
                selectedId = null;
                ClearInputs();
                try
                {
                    _isUpdating = true;

                    // Сбрасываем фильтрацию складов
                    var allStoragesView = _allStorages.DefaultView;
                    allStoragesView.RowFilter = string.Empty;
                    StorageBox.ItemsSource = allStoragesView;

                    // Сбрасываем фильтрацию продуктов
                    var allProductsView = _allProducts.DefaultView;
                    allProductsView.RowFilter = string.Empty;
                    ProductBox.ItemsSource = allProductsView;

                    // Очищаем выбранные значения
                    StorageBox.SelectedValue = null;
                    ProductBox.SelectedValue = null;

                    // Сбрасываем информацию о паллетах
                    PalletsBox.Tag = 0;
                    PalletsBox.Text = "";
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (StorageBox.SelectedValue == null || ProductBox.SelectedValue == null ||
                !int.TryParse(PalletsBox.Text, out int pallets) || pallets <= 0)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля корректно");
                return;
            }

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Проверяем наличие достаточного количества паллет
                int storageId = Convert.ToInt32(StorageBox.SelectedValue);
                int productId = Convert.ToInt32(ProductBox.SelectedValue);

                var checkCmd = new SQLiteCommand(
                    "SELECT Pallets_Stored FROM Stored WHERE Storage = @s AND Product = @p", conn, transaction);
                checkCmd.Parameters.AddWithValue("@s", storageId);
                checkCmd.Parameters.AddWithValue("@p", productId);
                var currentPallets = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (currentPallets < pallets)
                {
                    MessageBox.Show($"Недостаточно паллет на складе. Доступно: {currentPallets}");
                    return;
                }

                // 2. Добавляем запись о перемещении
                var insertCmd = new SQLiteCommand(@"
            INSERT INTO Transfer_Status 
            (Storage, Product, Recipient, Number_Of_Pallets, Status, Date_Of_Completion, Date_Of_Issue, Date_Of_Arrival)
            VALUES (@s, @p, @r, @n, @st, @dc, @di, @da)", conn, transaction);

                insertCmd.Parameters.AddWithValue("@s", storageId);
                insertCmd.Parameters.AddWithValue("@p", productId);
                insertCmd.Parameters.AddWithValue("@r", RecipientBox.SelectedValue ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@n", pallets);
                insertCmd.Parameters.AddWithValue("@st", (StatusBox.SelectedItem as ComboBoxItem)?.Tag ?? 1); // По умолчанию "Подготовка"
                insertCmd.Parameters.AddWithValue("@dc", CompletionDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                insertCmd.Parameters.AddWithValue("@di", IssueDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                insertCmd.Parameters.AddWithValue("@da", ArrivalDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));

                insertCmd.ExecuteNonQuery();

                // 3. Уменьшаем количество паллет на складе
                var updateCmd = new SQLiteCommand(
                    "UPDATE Stored SET Pallets_Stored = Pallets_Stored - @pallets " +
                    "WHERE Storage = @s AND Product = @p", conn, transaction);
                updateCmd.Parameters.AddWithValue("@s", storageId);
                updateCmd.Parameters.AddWithValue("@p", productId);
                updateCmd.Parameters.AddWithValue("@pallets", pallets);
                updateCmd.ExecuteNonQuery();

                transaction.Commit();
                MessageBox.Show("Заказ успешно создан и паллеты списаны со склада");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}");
                return;
            }

            LoadData();
            ClearInputs();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;
            if (!int.TryParse(PalletsBox.Text, out int newPallets) || newPallets <= 0)
            {
                MessageBox.Show("Укажите корректное количество паллет");
                return;
            }

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Получаем текущие данные о заказе
                var getCmd = new SQLiteCommand(
                    "SELECT Storage, Product, Number_Of_Pallets, Status FROM Transfer_Status WHERE id = @id",
                    conn, transaction);
                getCmd.Parameters.AddWithValue("@id", selectedId);
                using var reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("Заказ не найден");
                    return;
                }

                int storageId = reader.GetInt32(0);
                int productId = reader.GetInt32(1);
                int oldPallets = reader.GetInt32(2);
                int oldStatus = reader.GetInt32(3);
                reader.Close();

                // 2. Получаем новый статус
                int newStatus = (StatusBox.SelectedItem as ComboBoxItem)?.Tag != null ?
                    Convert.ToInt32((StatusBox.SelectedItem as ComboBoxItem).Tag) : oldStatus;

                // 3. Проверяем изменение количества паллет
                if (newPallets != oldPallets)
                {
                    // Проверяем наличие на складе
                    var checkCmd = new SQLiteCommand(
                        "SELECT Pallets_Stored FROM Stored WHERE Storage = @s AND Product = @p",
                        conn, transaction);
                    checkCmd.Parameters.AddWithValue("@s", storageId);
                    checkCmd.Parameters.AddWithValue("@p", productId);
                    int currentPallets = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (currentPallets + oldPallets < newPallets)
                    {
                        MessageBox.Show($"Недостаточно паллет на складе. Доступно: {currentPallets + oldPallets}");
                        return;
                    }

                    // Корректируем количество на складе
                    var updateStoreCmd = new SQLiteCommand(
                        "UPDATE Stored SET Pallets_Stored = Pallets_Stored + @old - @new " +
                        "WHERE Storage = @s AND Product = @p", conn, transaction);
                    updateStoreCmd.Parameters.AddWithValue("@s", storageId);
                    updateStoreCmd.Parameters.AddWithValue("@p", productId);
                    updateStoreCmd.Parameters.AddWithValue("@old", oldPallets);
                    updateStoreCmd.Parameters.AddWithValue("@new", newPallets);
                    updateStoreCmd.ExecuteNonQuery();
                }

                // 4. Обработка отмены заказа (возврат паллет)
                if (newStatus == 5 && oldStatus != 5) // Статус "Отмена"
                {
                    var returnCmd = new SQLiteCommand(
                        "UPDATE Stored SET Pallets_Stored = Pallets_Stored + @pallets " +
                        "WHERE Storage = @s AND Product = @p", conn, transaction);
                    returnCmd.Parameters.AddWithValue("@s", storageId);
                    returnCmd.Parameters.AddWithValue("@p", productId);
                    returnCmd.Parameters.AddWithValue("@pallets", newPallets);
                    returnCmd.ExecuteNonQuery();
                }
                // Обработка отмены отмены (повторное списание)
                else if (oldStatus == 5 && newStatus != 5)
                {
                    var deductCmd = new SQLiteCommand(
                        "UPDATE Stored SET Pallets_Stored = Pallets_Stored - @pallets " +
                        "WHERE Storage = @s AND Product = @p", conn, transaction);
                    deductCmd.Parameters.AddWithValue("@s", storageId);
                    deductCmd.Parameters.AddWithValue("@p", productId);
                    deductCmd.Parameters.AddWithValue("@pallets", newPallets);
                    deductCmd.ExecuteNonQuery();
                }

                // 5. Обновляем данные о заказе
                var updateCmd = new SQLiteCommand(@"
            UPDATE Transfer_Status
            SET Storage=@s, Product=@p, Recipient=@r, Number_Of_Pallets=@n, Status=@st,
                Date_Of_Completion=@dc, Date_Of_Issue=@di, Date_Of_Arrival=@da
            WHERE id=@id", conn, transaction);

                updateCmd.Parameters.AddWithValue("@id", selectedId);
                updateCmd.Parameters.AddWithValue("@s", StorageBox.SelectedValue ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@p", ProductBox.SelectedValue ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@r", RecipientBox.SelectedValue ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@n", newPallets);
                updateCmd.Parameters.AddWithValue("@st", newStatus);
                updateCmd.Parameters.AddWithValue("@dc", CompletionDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                updateCmd.Parameters.AddWithValue("@di", IssueDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                updateCmd.Parameters.AddWithValue("@da", ArrivalDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));

                updateCmd.ExecuteNonQuery();

                transaction.Commit();
                MessageBox.Show("Заказ успешно обновлен");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Ошибка при обновлении заказа: {ex.Message}");
                return;
            }

            LoadData();
            ClearInputs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            if (MessageBox.Show("Вы уверены, что хотите удалить этот заказ? Паллеты будут возвращены на склад.",
                "Подтверждение удаления", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Получаем данные о заказе для возврата паллет
                var getCmd = new SQLiteCommand(
                    "SELECT Storage, Product, Number_Of_Pallets, Status FROM Transfer_Status WHERE id = @id",
                    conn, transaction);
                getCmd.Parameters.AddWithValue("@id", selectedId);
                using var reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("Заказ не найден");
                    return;
                }

                int storageId = reader.GetInt32(0);
                int productId = reader.GetInt32(1);
                int pallets = reader.GetInt32(2);
                int status = reader.GetInt32(3);
                reader.Close();

                // 2. Возвращаем паллеты на склад (если заказ не был отменен)
                if (status != 5) // Если статус не "Отмена"
                {
                    var returnCmd = new SQLiteCommand(
                        "UPDATE Stored SET Pallets_Stored = Pallets_Stored + @pallets " +
                        "WHERE Storage = @s AND Product = @p", conn, transaction);
                    returnCmd.Parameters.AddWithValue("@s", storageId);
                    returnCmd.Parameters.AddWithValue("@p", productId);
                    returnCmd.Parameters.AddWithValue("@pallets", pallets);
                    returnCmd.ExecuteNonQuery();
                }

                // 3. Удаляем заказ
                var deleteCmd = new SQLiteCommand("DELETE FROM Transfer_Status WHERE id=@id", conn, transaction);
                deleteCmd.Parameters.AddWithValue("@id", selectedId);
                deleteCmd.ExecuteNonQuery();

                transaction.Commit();
                MessageBox.Show("Заказ удален. Паллеты возвращены на склад.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}");
                return;
            }

            LoadData();
            ClearInputs();
        }

        private void LoadForeignKeys()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            // Загружаем все склады
            var storages = new SQLiteDataAdapter("SELECT id, Code_Name FROM Storages", conn);
            _allStorages = new DataTable();
            storages.Fill(_allStorages);

            // Загружаем все продукты
            var products = new SQLiteDataAdapter("SELECT id, Name FROM Products", conn);
            _allProducts = new DataTable();
            products.Fill(_allProducts);

            // Получатели не фильтруются
            var recipients = new SQLiteDataAdapter("SELECT id, Name FROM Recipients", conn);
            var recipientTable = new DataTable();
            recipients.Fill(recipientTable);
            RecipientBox.ItemsSource = recipientTable.DefaultView;

            // Инициализируем комбобоксы
            StorageBox.ItemsSource = _allStorages.DefaultView;
            ProductBox.ItemsSource = _allProducts.DefaultView;
        }

        private void StorageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;

                int? currentStorageId = StorageBox.SelectedValue != null ?
                    Convert.ToInt32(StorageBox.SelectedValue) : (int?)null;

                // Сохраняем последний выбранный склад
                _lastStorageId = currentStorageId;

                if (currentStorageId.HasValue)
                {
                    // Фильтруем продукты по выбранному складу
                    var filteredProducts = _allProducts.Clone();
                    using (var conn = new SQLiteConnection("Data Source=databasepracti1.db"))
                    {
                        conn.Open();
                        var cmd = new SQLiteCommand(
                            "SELECT Product FROM Stored WHERE Storage = @storageId", conn);
                        cmd.Parameters.AddWithValue("@storageId", currentStorageId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int productId = reader.GetInt32(0);
                                DataRow[] rows = _allProducts.Select($"id = {productId}");
                                if (rows.Length > 0)
                                {
                                    filteredProducts.ImportRow(rows[0]);
                                }
                            }
                        }
                    }

                    // Сохраняем текущий выбранный продукт (если он доступен на новом складе)
                    int? currentProductId = ProductBox.SelectedValue != null ?
                        Convert.ToInt32(ProductBox.SelectedValue) : (int?)null;

                    ProductBox.ItemsSource = filteredProducts.DefaultView;

                    // Восстанавливаем выбор продукта, если он доступен на новом складе
                    if (currentProductId.HasValue &&
                        IsProductAvailableOnStorage(currentProductId.Value, currentStorageId.Value))
                    {
                        ProductBox.SelectedValue = currentProductId.Value;
                    }
                    else
                    {
                        ProductBox.SelectedValue = null;
                    }
                }
                else
                {
                    // Если склад не выбран, показываем все продукты
                    ProductBox.ItemsSource = _allProducts.DefaultView;
                }

                UpdateAvailablePallets();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void ProductBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;

                int? currentProductId = ProductBox.SelectedValue != null ?
                    Convert.ToInt32(ProductBox.SelectedValue) : (int?)null;

                // Сохраняем последний выбранный продукт
                _lastProductId = currentProductId;

                if (currentProductId.HasValue)
                {
                    // Фильтруем склады по выбранному продукту
                    var filteredStorages = _allStorages.Clone();
                    using (var conn = new SQLiteConnection("Data Source=databasepracti1.db"))
                    {
                        conn.Open();
                        var cmd = new SQLiteCommand(
                            "SELECT Storage FROM Stored WHERE Product = @productId", conn);
                        cmd.Parameters.AddWithValue("@productId", currentProductId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int storageId = reader.GetInt32(0);
                                DataRow[] rows = _allStorages.Select($"id = {storageId}");
                                if (rows.Length > 0)
                                {
                                    filteredStorages.ImportRow(rows[0]);
                                }
                            }
                        }
                    }

                    // Сохраняем текущий выбранный склад (если он доступен для нового продукта)
                    int? currentStorageId = StorageBox.SelectedValue != null ?
                        Convert.ToInt32(StorageBox.SelectedValue) : (int?)null;

                    StorageBox.ItemsSource = filteredStorages.DefaultView;

                    // Восстанавливаем выбор склада, если он доступен для нового продукта
                    if (currentStorageId.HasValue &&
                        IsProductAvailableOnStorage(currentProductId.Value, currentStorageId.Value))
                    {
                        StorageBox.SelectedValue = currentStorageId.Value;
                    }
                    else
                    {
                        StorageBox.SelectedValue = null;
                    }
                }
                else
                {
                    // Если продукт не выбран, показываем все склады
                    StorageBox.ItemsSource = _allStorages.DefaultView;

                    // Восстанавливаем последний выбранный склад, если он есть
                    if (_lastStorageId.HasValue)
                    {
                        StorageBox.SelectedValue = _lastStorageId.Value;
                    }
                }

                UpdateAvailablePallets();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private bool IsProductAvailableOnStorage(int productId, int storageId)
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(
                "SELECT COUNT(*) FROM Stored WHERE Product = @productId AND Storage = @storageId", conn);
            cmd.Parameters.AddWithValue("@productId", productId);
            cmd.Parameters.AddWithValue("@storageId", storageId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void UpdateAvailablePallets()
        {
            PalletsBox.Tag = 0;
            PalletsBox.Text = "";

            if (StorageBox.SelectedValue == null || ProductBox.SelectedValue == null)
                return;

            if (!int.TryParse(StorageBox.SelectedValue.ToString(), out int sId) ||
                !int.TryParse(ProductBox.SelectedValue.ToString(), out int pId))
                return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand(
                "SELECT Pallets_Stored FROM Stored WHERE Storage = @s AND Product = @p", conn);
            cmd.Parameters.AddWithValue("@s", sId);
            cmd.Parameters.AddWithValue("@p", pId);
            var r = cmd.ExecuteScalar();

            if (int.TryParse(r?.ToString(), out int max))
            {
                PalletsBox.Tag = max;
                PalletsBox.Text = max.ToString();
            }
        }
    }
}