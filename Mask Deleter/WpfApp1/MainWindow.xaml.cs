using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;


namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedDirectory = Environment.CurrentDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Метод для выбора директории
        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите директорию";
                dialog.SelectedPath = selectedDirectory;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    selectedDirectory = dialog.SelectedPath;
                    selectedDirectoryTextBox.Text = selectedDirectory;
                }
            }
        }

        // Метод для создания файла
        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string extension = extensionTextBox.Text;
                if (string.IsNullOrWhiteSpace(extension))
                {
                    System.Windows.MessageBox.Show("Введите расширение файла.");
                    return;
                }

                string baseFileName = "newfile";
                string fileName = $"{baseFileName}.{extension}";
                string filePath = System.IO.Path.Combine(selectedDirectory, fileName);

                // Если файл с таким именем существует, добавляем суффикс (1), (2), ...
                int count = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"{baseFileName}({count}).{extension}";
                    filePath = System.IO.Path.Combine(selectedDirectory, fileName);
                    count++;
                }

                // Создание файла
                File.Create(filePath).Close();
                System.Windows.MessageBox.Show($"Файл {fileName} создан в директории {selectedDirectory}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании файла: {ex.Message}");
            }
        }


        // Метод для удаления файлов по маске
        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mask = maskTextBox.Text;
                if (string.IsNullOrWhiteSpace(mask))
                {
                    System.Windows.MessageBox.Show("Введите маску поиска.");
                    return;
                }

                // Поиск файлов по маске в выбранной директории
                string[] files = Directory.GetFiles(selectedDirectory, mask);

                if (files.Length == 0)
                {
                    System.Windows.MessageBox.Show("Файлы не найдены.");
                    return;
                }

                // Собираем информацию об удалённых файлах
                string deletedFilesMessage = "Удалённые файлы:\n";
                foreach (string file in files)
                {
                    File.Delete(file);
                    deletedFilesMessage += $"{file}\n";
                }

                // Выводим одно сообщение со списком всех удалённых файлов
                System.Windows.MessageBox.Show(deletedFilesMessage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при удалении файлов: {ex.Message}");
            }
        }

    }
}
