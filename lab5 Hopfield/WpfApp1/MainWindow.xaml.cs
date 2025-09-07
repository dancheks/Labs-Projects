using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HopfieldNetworkApp
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 9; // Увеличили размер до 9x9
        private readonly Rectangle[,] cells = new Rectangle[GridSize, GridSize];
        private readonly int[,] weightMatrix = new int[GridSize * GridSize, GridSize * GridSize];
        private readonly int[][] trainedPatterns = new int[4][];
        private bool isMouseDown = false;
        private StringBuilder recognitionLog = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid();
            InitializeTrainedPatterns();
            var trainingErrors = TrainNetwork();
            LogTrainingResults(trainingErrors);
        }

        private void InitializeGrid()
        {
            for (int i = 0; i < GridSize; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    var cell = new Rectangle
                    {
                        Stroke = Brushes.Gray,
                        Fill = Brushes.White,
                        StrokeThickness = 0.5
                    };

                    cell.MouseDown += Cell_MouseDown;
                    cell.MouseEnter += Cell_MouseEnter;
                    cell.MouseUp += Cell_MouseUp;

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    MainGrid.Children.Add(cell);
                    cells[i, j] = cell;
                }
            }
        }

        private void InitializeTrainedPatterns()
        {
            // Буква A (9x9)
            trainedPatterns[0] = new int[] {
                0,0,0,1,1,1,0,0,0,
                0,0,1,0,0,0,1,0,0,
                0,1,0,0,0,0,0,1,0,
                0,1,0,0,0,0,0,1,0,
                0,1,0,0,0,0,0,1,0,
                0,1,1,1,1,1,1,1,0,
                0,1,0,0,0,0,0,1,0,
                0,1,0,0,0,0,0,1,0,
                0,1,0,0,0,0,0,1,0
            };

            // Буква B (9x9)
            trainedPatterns[1] = new int[] {
                1,1,1,1,1,1,1,0,0,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,1,0,
                1,1,1,1,1,1,1,0,0,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,1,0,
                1,1,1,1,1,1,1,0,0
            };

            // Буква C (9x9)
            trainedPatterns[2] = new int[] {
                0,0,1,1,1,1,1,0,0,
                0,1,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,0,1,
                1,0,0,0,0,0,0,0,0,
                1,0,0,0,0,0,0,0,0,
                1,0,0,0,0,0,0,0,0,
                1,0,0,0,0,0,0,0,1,
                0,1,0,0,0,0,0,1,0,
                0,0,1,1,1,1,1,0,0
            };

            // Буква D (9x9)
            trainedPatterns[3] = new int[] {
                1,1,1,1,1,1,0,0,0,
                1,0,0,0,0,0,1,0,0,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,0,0,1,
                1,0,0,0,0,0,0,0,1,
                1,0,0,0,0,0,0,0,1,
                1,0,0,0,0,0,0,1,0,
                1,0,0,0,0,0,1,0,0,
                1,1,1,1,1,1,0,0,0
            };
        }

        private double[] TrainNetwork()
        {
            int size = GridSize * GridSize;
            double[] trainingErrors = new double[trainedPatterns.Length];

            // Инициализация весовой матрицы нулями
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    weightMatrix[i, j] = 0;
                }
            }

            // Обучение сети (правило Хебба)
            for (int p = 0; p < trainedPatterns.Length; p++)
            {
                var pattern = trainedPatterns[p];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (i != j)
                        {
                            weightMatrix[i, j] += (2 * pattern[i] - 1) * (2 * pattern[j] - 1);
                        }
                    }
                }

                // Проверка качества обучения для текущего паттерна
                var recognized = RecognizePattern(pattern, false);
                trainingErrors[p] = CalculatePatternError(pattern, recognized);
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    weightMatrix[i, j] /= trainedPatterns.Length;
                }
            }
            return trainingErrors;
        }

        private double CalculatePatternError(int[] original, int[] recognized)
        {
            int errors = 0;
            for (int i = 0; i < original.Length; i++)
            {
                if (original[i] != recognized[i])
                {
                    errors++;
                }
            }
            return (double)errors / original.Length;
        }

        private void LogTrainingResults(double[] trainingErrors)
        {
            recognitionLog.Clear();
            recognitionLog.AppendLine("=== Результаты обучения ===");

            for (int i = 0; i < trainedPatterns.Length; i++)
            {
                char patternName = (char)('A' + i);
                recognitionLog.AppendLine($"Паттерн {patternName}: Ошибка = {trainingErrors[i]:P0}");
            }

            recognitionLog.AppendLine("\nСеть готова к распознаванию!");
            UpdateLog();
        }

        private int[] GetCurrentPattern()
        {
            int[] pattern = new int[GridSize * GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    pattern[i * GridSize + j] = cells[i, j].Fill == Brushes.Black ? 1 : 0;
                }
            }
            return pattern;
        }

        private void SetPattern(int[] pattern)
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    cells[i, j].Fill = pattern[i * GridSize + j] == 1 ? Brushes.Black : Brushes.White;
                }
            }
        }

        private int[] RecognizePattern(int[] inputPattern, bool logSteps = true)
        {
            recognitionLog.Clear();
            if (logSteps)
            {
                recognitionLog.AppendLine("=== Начало распознавания ===");
                recognitionLog.AppendLine("Исходный образ:");
                recognitionLog.AppendLine(PatternToString(inputPattern));
            }

            int size = GridSize * GridSize;
            int[] currentPattern = (int[])inputPattern.Clone();
            bool changed;
            int iteration = 0;

            do
            {
                changed = false;
                iteration++;

                if (logSteps)
                {
                    recognitionLog.AppendLine($"\nИтерация {iteration}:");
                }
                var randomOrder = Enumerable.Range(0, size).OrderBy(x => Guid.NewGuid()).ToList();
                foreach (var i in randomOrder)
                {
                    int sum = 0;
                    for (int j = 0; j < size; j++)
                    {
                        if (i != j)
                        {
                            sum += weightMatrix[i, j] * (2 * currentPattern[j] - 1);
                        }
                    }

                    int newValue = sum >= 0 ? 1 : 0;
                    if (newValue != currentPattern[i])
                    {
                        if (logSteps)
                        {
                            recognitionLog.AppendLine($"  Нейрон {i}: {currentPattern[i]} -> {newValue}");
                        }
                        currentPattern[i] = newValue;
                        changed = true;
                    }
                }

                if (logSteps && changed)
                {
                    recognitionLog.AppendLine("Текущее состояние:");
                    recognitionLog.AppendLine(PatternToString(currentPattern));
                }

            } while (changed && iteration < 100000); // Ограничение на число итераций

            if (logSteps)
            {
                recognitionLog.AppendLine("\n=== Распознавание завершено ===");
                recognitionLog.AppendLine($"Всего итераций: {iteration}");
                recognitionLog.AppendLine("Результат:");
                recognitionLog.AppendLine(PatternToString(currentPattern));

                // Определение, какой это паттерн
                int bestMatchIndex = -1;
                double bestMatchScore = 0;

                for (int i = 0; i < trainedPatterns.Length; i++)
                {
                    double similarity = CalculateSimilarity(currentPattern, trainedPatterns[i]);
                    recognitionLog.AppendLine($"Сходство с {((char)('A' + i))}: {similarity:P0}");

                    if (similarity > bestMatchScore)
                    {
                        bestMatchScore = similarity;
                        bestMatchIndex = i;
                    }
                }

                if (bestMatchIndex >= 0 && bestMatchScore > 0.7)
                {
                    recognitionLog.AppendLine($"\nНаиболее вероятно: это буква {(char)('A' + bestMatchIndex)}");
                }
                else
                {
                    recognitionLog.AppendLine("\nОбраз не распознан как один из обученных паттернов");
                }
            }

            UpdateLog();
            return currentPattern;
        }

        private double CalculateSimilarity(int[] pattern1, int[] pattern2)
        {
            int matches = 0;
            for (int i = 0; i < pattern1.Length; i++)
            {
                if (pattern1[i] == pattern2[i])
                {
                    matches++;
                }
            }
            return (double)matches / pattern1.Length;
        }

        private string PatternToString(int[] pattern)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    sb.Append(pattern[i * GridSize + j] == 1 ? "■ " : ". ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void UpdateLog()
        {
            RecognitionLogText.Text = recognitionLog.ToString();
            RecognitionLogScroll.ScrollToEnd();
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            ToggleCell(sender as Rectangle);
        }

        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                ToggleCell(sender as Rectangle);
            }
        }

        private void Cell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void ToggleCell(Rectangle cell)
        {
            if (cell != null)
            {
                cell.Fill = cell.Fill == Brushes.Black ? Brushes.White : Brushes.Black;
            }
        }

        private void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            var inputPattern = GetCurrentPattern();
            var recognizedPattern = RecognizePattern(inputPattern);
            SetPattern(recognizedPattern);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    cells[i, j].Fill = Brushes.White;
                }
            }
            recognitionLog.Clear();
            UpdateLog();
        }

        private void LoadPatternButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int index = int.Parse(button.Tag.ToString());
                SetPattern(trainedPatterns[index]);
            }
        }

        private void TestAllPatternsButton_Click(object sender, RoutedEventArgs e)
        {
            recognitionLog.Clear();
            recognitionLog.AppendLine("=== Тестирование всех паттернов ===");

            for (int i = 0; i < trainedPatterns.Length; i++)
            {
                char patternName = (char)('A' + i);
                recognitionLog.AppendLine($"\nТестирование паттерна {patternName}:");

                var recognized = RecognizePattern(trainedPatterns[i], false);
                double error = CalculatePatternError(trainedPatterns[i], recognized);

                recognitionLog.AppendLine($"Ошибка восстановления: {error:P0}");
                recognitionLog.AppendLine("Ожидаемый результат:");
                recognitionLog.AppendLine(PatternToString(trainedPatterns[i]));
                recognitionLog.AppendLine("Полученный результат:");
                recognitionLog.AppendLine(PatternToString(recognized));
            }

            UpdateLog();
        }
    }
}