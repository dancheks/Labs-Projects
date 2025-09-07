using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xceed.Words.NET; // Не забудьте добавить это пространство имен

namespace BusinessLetterGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Регулярное выражение для проверки формата номера телефона
            Regex regex = new Regex(@"^[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void GenerateLetterButton_Click(object sender, RoutedEventArgs e)
        {
            string recipientName = RecipientNameTextBox.Text;
            string date = DatePickerControl.SelectedDate?.ToString("dd.MM.yyyy") ?? "не указана";
            string phone = PhoneTextBox.Text;
            string senderName = SenderNameTextBox.Text;

            string templatePath = "Template.docx"; 
            string fileName = $"{recipientName}.docx";

            
            using (var document = DocX.Load(templatePath))
            {
                
                document.ReplaceText("{RecipientName}", recipientName, false, RegexOptions.None);
                document.ReplaceText("{Date}", date, false, RegexOptions.None);
                document.ReplaceText("{Phone}", phone, false, RegexOptions.None);
                document.ReplaceText("{SenderName}", senderName, false, RegexOptions.None);

                
                document.SaveAs(fileName);
            }

            MessageBox.Show($"Письмо успешно сгенерировано и сохранено как {fileName}");
        }
    }
}