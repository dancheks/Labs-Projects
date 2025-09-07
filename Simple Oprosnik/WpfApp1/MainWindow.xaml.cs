using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                return isChecked ? Brushes.Gray : Brushes.Transparent; // Измените цвет для отмеченного состояния
            }
            return Brushes.Transparent; // Цвет по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private List<QuestionStatus> questionStatuses;
        private bool[] correctAnswers; // Массив для хранения правильных ответов

        private void InitializeQuestionStatus()
        {
            questionStatuses = questions.Select((q, index) => new QuestionStatus
            {
                Index = index,
                DisplayText = $"Вопрос {index + 1}: Не отвечен",
                IsAnswered = false
            }).ToList();

            QuestionsList.ItemsSource = questionStatuses; // Привязываем к элементу QuestionsList
        }

        private int currentQuestionIndex = 0;
        private int score = 0;

        // Возможные ответы
        private List<string> allAnswers = new List<string>
        {
            "Бизнес-информация",
            "Монополия",
            "Теневой бизнес",
            "Венчурный бизнес",
            "Лизинг",
            "Франчайзинг",
            "Уставной фонд",
            "Трудовой договор",
            "Патент",
            "Электронная подпись"
        };

        // Вопросы
        private List<Question> questions = new List<Question>
        {
            new Question { Text = "Сведения о работе предприятия и его положении на рынке?", CorrectAnswer = "Бизнес-информация" },
            new Question { Text = "Организация, которая осуществляет контроль над ценой и объёмом предложения на рынке?", CorrectAnswer = "Монополия" },
            new Question { Text = "Экономические взаимоотношения граждан общества, развивающиеся стихийно, в обход существующих государственных законов и общественных правил?", CorrectAnswer = "Теневой бизнес" },
            new Question { Text = "Рискованный научно-технический или технологический бизнес?", CorrectAnswer = "Венчурный бизнес" },
            new Question { Text = "Вид финансовых услуг, форма кредитования для приобретения основных средств предприятиями?", CorrectAnswer = "Лизинг" },
            new Question { Text = "«Аренда» товарного знака или коммерческого обозначения?", CorrectAnswer = "Франчайзинг" },
            new Question { Text = "Вклад учредителей компании в совокупности?", CorrectAnswer = "Уставной фонд" },
            new Question { Text = "Соглашение между работником и нанимателем, в соответствии с которым работник обязуется выполнять работу?", CorrectAnswer = "Трудовой договор" },
            new Question { Text = "Исключительное право, предоставляемое на изобретение?", CorrectAnswer = "Патент" },
            new Question { Text = "Подпись, позволяющая подтвердить подлинность электронного документа?", CorrectAnswer = "Электронная подпись" }
        };

        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            correctAnswers = new bool[questions.Count];
            LoadQuestion();
            InitializeQuestionStatus();
        }


        private void UpdateQuestionStatus(int questionIndex, bool isAnswered)
        {
            var status = questionStatuses[questionIndex];
            status.IsAnswered = isAnswered;
            status.DisplayText = $"Вопрос {questionIndex + 1}: {(isAnswered ? "Отвечен" : "Не отвечен")}";
            QuestionsList.Items.Refresh();
        }


        // Загрузка текущего вопроса с рандомизированными вариантами
        private void LoadQuestion()
        {
            if (currentQuestionIndex < questions.Count)
            {
                var question = questions[currentQuestionIndex];
                QuestionText.Text = question.Text;

                // Если варианты ответов еще не были сгенерированы
                if (!question.IsAnswerOptionsGenerated)
                {
                    // Генерация вариантов ответов
                    List<string> options = new List<string>();
                    options.Add(question.CorrectAnswer);

                    // Добавляем случайные ответы, исключая правильный
                    options.AddRange(allAnswers.Where(answer => answer != question.CorrectAnswer)
                        .OrderBy(x => random.Next()).Take(3));

                    // Перемешиваем варианты
                    options = options.OrderBy(x => random.Next()).ToList();

                    // Присваиваем сгенерированные варианты
                    question.AnswerOptions = options;
                    question.IsAnswerOptionsGenerated = true; // Устанавливаем флаг, что варианты были сгенерированы
                }

                // Присваиваем варианты кнопкам из уже сгенерированного списка
                Option1.Content = question.AnswerOptions[0];
                Option2.Content = question.AnswerOptions[1];
                Option3.Content = question.AnswerOptions[2];
                Option4.Content = question.AnswerOptions[3];

                // Обновляем индекс правильного ответа
                questions[currentQuestionIndex].CorrectOption = question.AnswerOptions.IndexOf(question.CorrectAnswer);

                // Восстанавливаем выбор RadioButton, если ответ был дан
                ResetRadioButtons();
                if (question.UserAnswer.HasValue)
                {
                    switch (question.UserAnswer.Value)
                    {
                        case 0:
                            Option1.IsChecked = true;
                            break;
                        case 1:
                            Option2.IsChecked = true;
                            break;
                        case 2:
                            Option3.IsChecked = true;
                            break;
                        case 3:
                            Option4.IsChecked = true;
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show($"Тест завершен! Ваш результат: {score} баллов.");
                ResetTest();
            }
        }

        private void ResetTest()
        {
            foreach (var question in questions)
            {
                question.IsAnswerOptionsGenerated = false; // Сбрасываем флаг
                question.AnswerOptions.Clear(); // Очищаем предыдущие варианты
                question.UserAnswer = null;
            }

            // Сбрасываем значения в массиве правильных ответов
            Array.Clear(correctAnswers, 0, correctAnswers.Length);

            int qnum = questions.Count; // Обновляем количество вопросов
            for (int i = 0; i < qnum; i++)
            {
                UpdateQuestionStatus(i, false);
            }

            currentQuestionIndex = 0;
            score = 0;
            LoadQuestion();
            ScoreText.Text = $"Ваши баллы: {score}";
        }




        // Сброс выбора RadioButton
        private void ResetRadioButtons()
        {
            Option1.IsChecked = false;
            Option2.IsChecked = false;
            Option3.IsChecked = false;
            Option4.IsChecked = false;
        }

        // Проверка ответа
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedOption = -1;

            if (Option1.IsChecked == true) selectedOption = 0;
            if (Option2.IsChecked == true) selectedOption = 1;
            if (Option3.IsChecked == true) selectedOption = 2;
            if (Option4.IsChecked == true) selectedOption = 3;

            // Сохраняем ответ пользователя
            questions[currentQuestionIndex].UserAnswer = selectedOption;

            // Проверяем правильность ответа и обновляем массив
            bool isCorrect = selectedOption == questions[currentQuestionIndex].CorrectOption;
            correctAnswers[currentQuestionIndex] = isCorrect;

            UpdateQuestionStatus(currentQuestionIndex, true);

            // Обновляем счет
            score = correctAnswers.Count(c => c) * 10; // Умножаем количество правильных ответов на 10

            currentQuestionIndex++;
            ScoreText.Text = $"Ваши баллы: {score}";

            LoadQuestion();
        }

        private void QuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int questionIndex)
            {
                currentQuestionIndex = questionIndex;
                LoadQuestion();
            }
        }

    }


    public class Question
    {
        public string Text { get; set; }
        public string CorrectAnswer { get; set; }
        public int CorrectOption { get; set; }
        public List<string> AnswerOptions { get; set; } = new List<string>();
        public bool IsAnswerOptionsGenerated { get; set; } = false; // Новый флаг
        public int? UserAnswer { get; set; } // Новое свойство для хранения ответа пользователя
    }

    public class QuestionStatus
    {
        public int Index { get; set; }
        public string DisplayText { get; set; }
        public bool IsAnswered { get; set; }
    }

}
