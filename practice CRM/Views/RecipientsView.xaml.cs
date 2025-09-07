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
using System.Text.RegularExpressions;

namespace SqlliteViewer.Views
{
    /// <summary>
    /// Логика взаимодействия для RecipientsView.xaml
    /// </summary>
    public partial class RecipientsView : UserControl
    {
        private int? selectedId = null;

        public RecipientsView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Recipients", conn);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            RecipientsGrid.ItemsSource = dt.DefaultView;
            RecipientsGrid.SelectedIndex = -1;
            selectedId = null;
        }

        private void ClearInputs()
        {
            NameBox.Text = "";
            ContactNumberBox.Text = "";
            AddressBox.Text = "";
        }

        private void RecipientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecipientsGrid.SelectedItem is DataRowView row)
            {
                selectedId = Convert.ToInt32(row["id"]);
                NameBox.Text = row["Name"].ToString();
                ContactNumberBox.Text = row["Contact_Number"].ToString();
                AddressBox.Text = row["Address"].ToString();

                // Определяем префикс (например, +375)
                string phone = row["Contact_Number"].ToString();
                var match = Regex.Match(phone, @"^\+(\d+)");
                if (match.Success)
                {
                    string code = match.Groups[1].Value;

                    // Ищем страну с таким кодом
                    foreach (ComboBoxItem item in CountryBox.Items)
                    {
                        if ((item.Tag?.ToString() ?? "") == code)
                        {
                            CountryBox.SelectedItem = item;
                            break;
                        }
                    }
                }
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
                INSERT INTO Recipients (Name, Contact_Number, Address)
                VALUES (@n, @c, @a)", conn);

            cmd.Parameters.AddWithValue("@n", NameBox.Text);
            cmd.Parameters.AddWithValue("@c", ContactNumberBox.Text);
            cmd.Parameters.AddWithValue("@a", AddressBox.Text);
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
                UPDATE Recipients
                SET Name=@n, Contact_Number=@c, Address=@a
                WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.Parameters.AddWithValue("@n", NameBox.Text);
            cmd.Parameters.AddWithValue("@c", ContactNumberBox.Text);
            cmd.Parameters.AddWithValue("@a", AddressBox.Text);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId is null) return;

            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM Recipients WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", selectedId);
            cmd.ExecuteNonQuery();

            LoadData();
            ClearInputs();
        }

        private void CountryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CountryBox.SelectedItem is ComboBoxItem selected)
            {
                var prefix = "+" + (selected.Tag?.ToString() ?? "375");

                // Если поле телефона пустое или не начинается с префикса — подставим
                if (string.IsNullOrWhiteSpace(ContactNumberBox.Text) || !ContactNumberBox.Text.StartsWith("+"))
                {
                    ContactNumberBox.Text = prefix + " ";
                    ContactNumberBox.CaretIndex = ContactNumberBox.Text.Length;
                }
                // Или заменить старый префикс на новый
                else
                {
                    var digits = Regex.Replace(ContactNumberBox.Text, @"^\+\d+", ""); // убираем старый префикс
                    ContactNumberBox.Text = prefix + " " + digits.TrimStart();
                    ContactNumberBox.CaretIndex = ContactNumberBox.Text.Length;
                }
            }
        }
        private bool _handlingTextChanged = false;

        private void ContactNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_handlingTextChanged) return;
            _handlingTextChanged = true;

            // Извлекаем текущий префикс
            string prefix = "+375";
            if (CountryBox.SelectedItem is ComboBoxItem selected)
                prefix = "+" + (selected.Tag?.ToString() ?? "375");

            // Убираем всё, кроме цифр после префикса
            string digitsOnly = Regex.Replace(ContactNumberBox.Text, @"[^\d]", "");

            if (digitsOnly.StartsWith(prefix.TrimStart('+')))
                digitsOnly = digitsOnly.Substring(prefix.Length - 1);

            // Максимум 9 цифр (после префикса)
            if (digitsOnly.Length > 9)
                digitsOnly = digitsOnly.Substring(0, 9);

            string formatted = FormatBelarusPhone(prefix, digitsOnly);
            ContactNumberBox.Text = formatted;
            ContactNumberBox.CaretIndex = formatted.Length;

            _handlingTextChanged = false;
        }
        private string FormatBelarusPhone(string prefix, string digits)
        {
            if (digits.Length <= 2)
                return $"{prefix} ({digits}";
            if (digits.Length <= 5)
                return $"{prefix} ({digits[..2]}) {digits[2..]}";
            if (digits.Length <= 7)
                return $"{prefix} ({digits[..2]}) {digits[2..5]}-{digits[5..]}";
            if (digits.Length <= 9)
                return $"{prefix} ({digits[..2]}) {digits[2..5]}-{digits[5..7]}-{digits[7..]}";

            return $"{prefix} ({digits[..2]}) {digits[2..5]}-{digits[5..7]}-{digits[7..9]}";
        }

    }
}
