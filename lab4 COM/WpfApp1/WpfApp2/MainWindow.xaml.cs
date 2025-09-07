using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelEditor
{
    public partial class MainWindow : Window
    {
        private DataTable dataTable;
        private DataTable summaryTable = new DataTable();


        public MainWindow()
        {
            InitializeComponent();
            CreateDataTableStructure();
            ConfigureDataGrid();
            CreateSummaryTable();
            ConfigureSummaryGrid();
            UpdateSummary();

        }
        private void CreateSummaryTable()
        {
            summaryTable.Columns.Add("Отдел", typeof(string));
            summaryTable.Columns.Add("Код", typeof(string));
            summaryTable.Columns.Add("Накладные", typeof(double));
            summaryTable.Columns.Add("Материалы", typeof(double));
            summaryTable.Columns.Add("Зарплата", typeof(double));
            summaryTable.Columns.Add("Себестоимость", typeof(double));

            var row = summaryTable.NewRow();
            row["Отдел"] = "Итого";
            summaryTable.Rows.Add(row);
        }

        private void ConfigureSummaryGrid()
        {
            summaryGrid.AutoGenerateColumns = false;

            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отдел",
                Binding = new Binding("Отдел")
            });
            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Код изделия",
                Binding = new Binding("Код")
            });
            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Накладные затраты",
                Binding = new Binding("Накладные")
            });
            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Затраты на материал",
                Binding = new Binding("Материалы")
            });
            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Затраты на ЗП",
                Binding = new Binding("Зарплата")
            });
            summaryGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Себестоимость",
                Binding = new Binding("Себестоимость")
            });

            summaryGrid.ItemsSource = summaryTable.DefaultView;
        }
        private void UpdateSummary()
        {
            double totalOverhead = 0;
            double totalMaterial = 0;
            double totalSalary = 0;
            double totalCost = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    totalOverhead += row.Field<double>("Накладные");
                    totalMaterial += row.Field<double>("Материалы");
                    totalSalary += row.Field<double>("Зарплата");
                    totalCost += row.Field<double>("Себестоимость");
                }
            }

            var summaryRow = summaryTable.Rows[0];
            summaryRow["Накладные"] = totalOverhead;
            summaryRow["Материалы"] = totalMaterial;
            summaryRow["Зарплата"] = totalSalary;
            summaryRow["Себестоимость"] = totalCost;
        }



        private void CreateDataTableStructure()
        {
            dataTable = new DataTable();

            // Добавляем столбцы
            dataTable.Columns.Add("Отдел", typeof(string));
            dataTable.Columns.Add("Код", typeof(int));
            dataTable.Columns.Add("Накладные", typeof(double));
            dataTable.Columns.Add("Материалы", typeof(double));
            dataTable.Columns.Add("Зарплата", typeof(double));

            // Вычисляемый столбец
            dataTable.Columns.Add("Себестоимость", typeof(double), "[Накладные] + [Материалы] + [Зарплата]");
        }

        private void ConfigureDataGrid()
        {
            dataGrid.AutoGenerateColumns = false;

            // Настройка колонок с мгновенным обновлением
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отдел",
                Binding = new Binding("Отдел") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Код изделия",
                Binding = new Binding("Код") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Накладные затраты",
                Binding = new Binding("Накладные") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Затраты на материал",
                Binding = new Binding("Материалы") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Затраты на ЗП",
                Binding = new Binding("Зарплата") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Себестоимость",
                Binding = new Binding("Себестоимость") { Mode = BindingMode.OneWay }
            });

            dataGrid.ItemsSource = dataTable.DefaultView;
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
            dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var textBox = e.EditingElement as TextBox;
                var binding = textBox?.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();

                dataTable.AcceptChanges();
                UpdateSummary();
            }
        }


        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var currentCell = dataGrid.CurrentCell;
                if (currentCell.Column == null) return;

                int columnIndex = currentCell.Column.DisplayIndex;
                int rowIndex = dataGrid.Items.IndexOf(currentCell.Item);

                if (columnIndex == dataGrid.Columns.Count - 1 &&
                    rowIndex == dataGrid.Items.Count - 1)
                {
                    AddNewRow();
                    e.Handled = true;
                }
            }
        }

        private void AddNewRow()
        {
            var newRow = dataTable.NewRow();
            newRow["Накладные"] = 0.0;
            newRow["Материалы"] = 0.0;
            newRow["Зарплата"] = 0.0;
            dataTable.Rows.Add(newRow);

            // Фокус на новую строку
            Dispatcher.BeginInvoke(new Action(() =>
            {
                dataGrid.ScrollIntoView(newRow);
                dataGrid.CurrentCell = new DataGridCellInfo(newRow, dataGrid.Columns[0]);
                dataGrid.BeginEdit();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void LoadExcel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadDataFromExcel(openFileDialog.FileName);
            }
        }

        private void LoadDataFromExcel(string filePath)
        {
            try
            {
                var excelApp = new Excel.Application();
                var workbook = excelApp.Workbooks.Open(filePath);
                var worksheet = (Excel.Worksheet)workbook.Sheets[1];

                dataTable.Rows.Clear();

                int row = 3; // Начинаем с 3 строки (где данные)
                while (((Excel.Range)worksheet.Cells[row, 1]).Value != null)
                {
                    var newRow = dataTable.NewRow();
                    newRow["Отдел"] = ((Excel.Range)worksheet.Cells[row, 1]).Value;
                    newRow["Код"] = ((Excel.Range)worksheet.Cells[row, 2]).Value;
                    newRow["Накладные"] = ((Excel.Range)worksheet.Cells[row, 3]).Value;
                    newRow["Материалы"] = ((Excel.Range)worksheet.Cells[row, 4]).Value;
                    newRow["Зарплата"] = ((Excel.Range)worksheet.Cells[row, 5]).Value;
                    dataTable.Rows.Add(newRow);
                    row++;
                }

                workbook.Close(false);
                excelApp.Quit();

                // Освобождаем COM-объекты
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void SaveExcel_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveDataToExcel(saveFileDialog.FileName);
            }
        }

        /*private void SaveDataToExcel(string filePath)
        {
            try
            {
                var excelApp = new Excel.Application();
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Excel.Worksheet)workbook.Sheets[1];

                // Заголовки
                worksheet.Cells[1, 1] = "Себестоимость опытно-экспериментальных работ";

                // Шапка таблицы
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    worksheet.Cells[2, i + 1] = dataGrid.Columns[i].Header;
                }

                // Данные
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var row = dataTable.Rows[i];
                    worksheet.Cells[i + 3, 1] = row["Отдел"];
                    worksheet.Cells[i + 3, 2] = row["Код"];
                    worksheet.Cells[i + 3, 3] = row["Накладные"];
                    worksheet.Cells[i + 3, 4] = row["Материалы"];
                    worksheet.Cells[i + 3, 5] = row["Зарплата"];
                    worksheet.Cells[i + 3, 6].Formula = $"=C{i + 3}+D{i + 3}+E{i + 3}";
                }

                // Формулы для итогов
                int lastRow = dataTable.Rows.Count + 2;
                worksheet.Cells[lastRow + 1, 1] = "Итого";
                worksheet.Cells[lastRow + 1, 2].Formula = $"=SUM(B3:B{lastRow})";
                worksheet.Cells[lastRow + 1, 3].Formula = $"=SUM(C3:C{lastRow})";
                worksheet.Cells[lastRow + 1, 4].Formula = $"=SUM(D3:D{lastRow})";
                worksheet.Cells[lastRow + 1, 5].Formula = $"=SUM(E3:E{lastRow})";
                worksheet.Cells[lastRow + 1, 6].Formula = $"=SUM(F3:F{lastRow})";

                // Форматирование
                Excel.Range range = worksheet.Range["A2", $"F{lastRow + 1}"];
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                Excel.Range headerRange = worksheet.Range["A2", "F2"];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = Excel.XlRgbColor.rgbLightGray;

                // Автоподбор ширины столбцов
                worksheet.Columns.AutoFit();

                // Сохранение
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                // Освобождаем COM-объекты
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(headerRange);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                MessageBox.Show("Файл успешно сохранен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }*/

        private void SaveDataToExcel(string filePath)
        {
            try
            {
                var excelApp = new Excel.Application();
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Excel.Worksheet)workbook.Sheets[1];

                // Заголовки
                worksheet.Cells[1, 1] = "Себестоимость опытно-экспериментальных работ";

                // Шапка таблицы
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    worksheet.Cells[2, i + 1] = dataGrid.Columns[i].Header;
                }

                // Данные
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var row = dataTable.Rows[i];
                    worksheet.Cells[i + 3, 1] = row["Отдел"];
                    worksheet.Cells[i + 3, 2] = row["Код"];
                    worksheet.Cells[i + 3, 3] = row["Накладные"];
                    worksheet.Cells[i + 3, 4] = row["Материалы"];
                    worksheet.Cells[i + 3, 5] = row["Зарплата"];
                    worksheet.Cells[i + 3, 6].Formula = $"=C{i + 3}+D{i + 3}+E{i + 3}";
                }

                // Формулы для итогов
                int lastRow = dataTable.Rows.Count + 2;
                worksheet.Cells[lastRow + 1, 1] = "Итого";
                worksheet.Cells[lastRow + 1, 2].Formula = $"=SUM(B3:B{lastRow})";
                worksheet.Cells[lastRow + 1, 3].Formula = $"=SUM(C3:C{lastRow})";
                worksheet.Cells[lastRow + 1, 4].Formula = $"=SUM(D3:D{lastRow})";
                worksheet.Cells[lastRow + 1, 5].Formula = $"=SUM(E3:E{lastRow})";
                worksheet.Cells[lastRow + 1, 6].Formula = $"=SUM(F3:F{lastRow})";

                // Форматирование
                Excel.Range range = worksheet.Range["A2", $"F{lastRow + 1}"];
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                Excel.Range headerRange = worksheet.Range["A2", "F2"];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = Excel.XlRgbColor.rgbLightGray;

                // Автоподбор ширины столбцов
                worksheet.Columns.AutoFit();

                // Создание гистограммы
                Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                Excel.ChartObject chartObject = chartObjects.Add(750, 50, 400, 300); // Позиция и размер графика
                Excel.Chart chart = chartObject.Chart;

                // Установка данных для графика
                chart.ChartType = Excel.XlChartType.xlColumnClustered; // Тип графика - гистограмма

                // Добавление серий для каждого отдела
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var row = dataTable.Rows[i];
                    string departmentName = row.Field<string>("Отдел");
                    double cost = row.Field<double>("Себестоимость");

                    // Добавляем новую серию
                    Excel.Series series = (Excel.Series)chart.SeriesCollection().NewSeries();
                    series.Name = departmentName; // Название серии
                    series.Values = new double[] { cost }; // Значение для серии
                    series.XValues = new string[] { departmentName }; // Название для оси X
                }

                // Заголовок графика
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Гистограмма себестоимости";

                // Сохранение
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                // Освобождаем COM-объекты
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(headerRange);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                MessageBox.Show("Файл успешно сохранен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            AddNewRow();
        }
    }
}