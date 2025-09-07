using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace SqlliteViewer.Views
{
    public partial class StatisticsView : UserControl
    {
        public SeriesCollection StatusSeries { get; set; }
        public SeriesCollection TopProductsSeries { get; set; }
        public SeriesCollection MonthlySalesSeries { get; set; }
        public SeriesCollection RecipientActivitySeries { get; set; }

        public List<string> StatusLabels { get; set; }
        public List<string> TopProductsLabels { get; set; }
        public List<string> MonthlySalesLabels { get; set; }
        public List<string> RecipientActivityLabels { get; set; }

        public string SummaryText { get; set; }

        public StatisticsView()
        {
            InitializeComponent();
            LoadData();
            this.DataContext = this;
        }

        private void LoadData()
        {
            using var conn = new SQLiteConnection("Data Source=databasepracti1.db");
            conn.Open();

            LoadStatusStatistics(conn);
            LoadTopProducts(conn);
            LoadMonthlySales(conn);
            LoadRecipientActivity(conn);
            LoadSummaryStatistics(conn);
        }

        private void LoadStatusStatistics(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand(@"
                SELECT Status, COUNT(*) as Count
                FROM Transfer_Status
                GROUP BY Status", conn);

            var table = new DataTable();
            new SQLiteDataAdapter(cmd).Fill(table);

            var statusMap = new Dictionary<int, string>
            {
                { 1, "Подготовка" },
                { 2, "Идёт" },
                { 3, "Прибыл" },
                { 4, "Задержан" },
                { 5, "Отмена" }
            };

            var labels = new List<string>();
            var values = new ChartValues<int>();

            foreach (DataRow row in table.Rows)
            {
                int code = Convert.ToInt32(row["Status"]);
                int count = Convert.ToInt32(row["Count"]);

                labels.Add(statusMap.ContainsKey(code) ? statusMap[code] : $"Код {code}");
                values.Add(count);
            }

            StatusLabels = labels;

            StatusSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Количество",
                    Values = values,
                    Fill = System.Windows.Media.Brushes.SteelBlue
                }
            };
        }

        private void LoadTopProducts(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand(@"
                SELECT p.Name, SUM(ts.Number_Of_Pallets) as TotalPallets, 
                       SUM(ts.Number_Of_Pallets * p.Number_In_Pallet) as TotalItems
                FROM Transfer_Status ts
                JOIN Products p ON ts.Product = p.id
                WHERE ts.Status != 5  -- Исключаем отмененные
                GROUP BY p.Name
                ORDER BY TotalPallets DESC
                LIMIT 5", conn);

            var table = new DataTable();
            new SQLiteDataAdapter(cmd).Fill(table);

            var labels = new List<string>();
            var palletValues = new ChartValues<int>();
            var itemValues = new ChartValues<int>();

            foreach (DataRow row in table.Rows)
            {
                labels.Add(row["Name"].ToString());
                palletValues.Add(Convert.ToInt32(row["TotalPallets"]));
                itemValues.Add(Convert.ToInt32(row["TotalItems"]));
            }

            TopProductsLabels = labels;

            TopProductsSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Паллеты",
                    Values = palletValues,
                    Fill = System.Windows.Media.Brushes.Tomato
                },
                new ColumnSeries
                {
                    Title = "Штуки",
                    Values = itemValues,
                    Fill = System.Windows.Media.Brushes.Gold
                }
            };
        }

        private void LoadMonthlySales(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand(@"
                SELECT strftime('%Y-%m', Date_Of_Issue) as Month, 
                       SUM(Number_Of_Pallets) as TotalPallets
                FROM Transfer_Status
                WHERE Status != 5  -- Исключаем отмененные
                GROUP BY strftime('%Y-%m', Date_Of_Issue)
                ORDER BY Month", conn);

            var table = new DataTable();
            new SQLiteDataAdapter(cmd).Fill(table);

            var labels = new List<string>();
            var values = new ChartValues<int>();

            foreach (DataRow row in table.Rows)
            {
                labels.Add(row["Month"].ToString());
                values.Add(Convert.ToInt32(row["TotalPallets"]));
            }

            MonthlySalesLabels = labels;

            MonthlySalesSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Продажи (паллеты)",
                    Values = values,
                    Stroke = System.Windows.Media.Brushes.DodgerBlue,
                    Fill = System.Windows.Media.Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10
                }
            };
        }

        private void LoadRecipientActivity(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand(@"
        SELECT r.Name, COUNT(*) as OrderCount
        FROM Transfer_Status ts
        JOIN Recipients r ON ts.Recipient = r.id
        WHERE ts.Status != 5  -- исключаем отменённые
        GROUP BY r.Name
        ORDER BY OrderCount DESC
        LIMIT 5", conn);

            var table = new DataTable();
            new SQLiteDataAdapter(cmd).Fill(table);

            RecipientActivitySeries = new SeriesCollection();

            foreach (DataRow row in table.Rows)
            {
                string name = row["Name"].ToString();
                int count = Convert.ToInt32(row["OrderCount"]);

                RecipientActivitySeries.Add(new PieSeries
                {
                    Title = name, // отображается в легенде
                    Values = new ChartValues<int> { count },
                    DataLabels = true,
                    LabelPoint = cp => $"{cp.Y}", // только число заказов
                    LabelPosition = PieLabelPosition.OutsideSlice,
                    StrokeThickness = 0
                });
            }
        }


        private void LoadSummaryStatistics(SQLiteConnection conn)
        {
            var cmd = new SQLiteCommand(@"
                SELECT 
                    SUM(ts.Number_Of_Pallets) as TotalPallets,
                    SUM(ts.Number_Of_Pallets * p.Number_In_Pallet) as TotalItems,
                    (SELECT p.Name FROM Products p 
                     JOIN Transfer_Status ts ON p.id = ts.Product
                     WHERE ts.Status != 5
                     GROUP BY p.Name
                     ORDER BY SUM(ts.Number_Of_Pallets) DESC
                     LIMIT 1) as TopProduct,
                    (SELECT r.Name FROM Recipients r 
                     JOIN Transfer_Status ts ON r.id = ts.Recipient
                     WHERE ts.Status != 5
                     GROUP BY r.Name
                     ORDER BY COUNT(*) DESC
                     LIMIT 1) as TopRecipient
                FROM Transfer_Status ts
                JOIN Products p ON ts.Product = p.id
                WHERE ts.Status != 5", conn);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int totalPallets = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                int totalItems = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                string topProduct = reader.IsDBNull(2) ? "нет данных" : reader.GetString(2);
                string topRecipient = reader.IsDBNull(3) ? "нет данных" : reader.GetString(3);

                SummaryText = $"Общая статистика:\n\n" +
                             $"Всего отгружено паллет: {totalPallets}\n" +
                             $"Всего отгружено штук: {totalItems}\n" +
                             $"Самый популярный товар: {topProduct}\n" +
                             $"Самый активный получатель: {topRecipient}";
            }
            else
            {
                SummaryText = "Нет данных для отображения";
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var excel = new Excel.Application();
                excel.Visible = false;

                var wb = excel.Workbooks.Add();

                // Лист со сводной статистикой
                var summarySheet = (Excel.Worksheet)wb.Worksheets[1];
                summarySheet.Name = "Сводка";
                summarySheet.Cells[1, 1] = "Сводная статистика";

                int row = 2;
                foreach (var line in SummaryText.Split('\n'))
                {
                    summarySheet.Cells[row++, 1] = line;
                }

                // Лист со статусами
                var statusSheet = (Excel.Worksheet)wb.Worksheets.Add();
                statusSheet.Name = "Статусы";
                statusSheet.Cells[1, 1] = "Статус";
                statusSheet.Cells[1, 2] = "Количество";

                for (int i = 0; i < StatusLabels.Count; i++)
                {
                    statusSheet.Cells[i + 2, 1] = StatusLabels[i];
                    statusSheet.Cells[i + 2, 2] = ((ColumnSeries)StatusSeries[0]).Values[i];
                }

                // Лист с популярными товарами
                var productsSheet = (Excel.Worksheet)wb.Worksheets.Add();
                productsSheet.Name = "Товары";
                productsSheet.Cells[1, 1] = "Товар";
                productsSheet.Cells[1, 2] = "Паллеты";
                productsSheet.Cells[1, 3] = "Штуки";

                for (int i = 0; i < TopProductsLabels.Count; i++)
                {
                    productsSheet.Cells[i + 2, 1] = TopProductsLabels[i];
                    productsSheet.Cells[i + 2, 2] = ((ColumnSeries)TopProductsSeries[0]).Values[i];
                    productsSheet.Cells[i + 2, 3] = ((ColumnSeries)TopProductsSeries[1]).Values[i];
                }

                // Сохранение
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string path = System.IO.Path.Combine(desktop, "Статистика_перемещений.xlsx");
                wb.SaveAs(path);
                wb.Close();
                excel.Quit();

                MessageBox.Show($"Файл сохранён:\n{path}", "Экспорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}