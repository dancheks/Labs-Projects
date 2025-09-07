using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using static System.Math;
public enum MemoryMode { GET, SET }
public enum NeuronType { Hidden, Output }

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlotModelViewModel plotModel;
        private Network network;
        public MainWindow()
        {
            InitializeComponent();
            network = new Network();
            plotModel = new PlotModelViewModel();
            PlotView.Model = plotModel.Model;
        }

        public void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button cell = sender as Button;
            if (cell.Background == System.Windows.Media.Brushes.White)
            {
                cell.Background = System.Windows.Media.Brushes.Black;
            }
            else
            {
                cell.Background = System.Windows.Media.Brushes.White;
            }
        }
        public void zap_f_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(0, newLetter); // Обновляем букву A
            MessageBox.Show("Записана буква 1");
        }

        public void zap_s_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(1, newLetter); // Обновляем букву B
            MessageBox.Show("Записана буква 2");
        }

        public void zap_t_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(2, newLetter); // Обновляем букву C
            MessageBox.Show("Записана буква 3");
        }

        public void zap_fh_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(3, newLetter); // Обновляем букву D
            MessageBox.Show("Записана буква 4");
        }

        public void zap_ff_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(4, newLetter); // Обновляем букву E
            MessageBox.Show("Записана буква 5");
        }

        public void zap_ss_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(5, newLetter); // Обновляем букву F
            MessageBox.Show("Записана буква 6");
        }

        public void zap_se_Click(object sender, RoutedEventArgs e)
        {
            int[,] newLetter = Find(); // Получаем текущее состояние нарисованной буквы
            network.input_layer.UpdateLetter(6, newLetter); // Обновляем букву G
            MessageBox.Show("Записана буква 7");
        }
        public int[,] Find()
        {
            int[,] input = new int[8, 8];
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Button cell = (Button)grid.Children[i * 7 + j];
                    input[i, j] = cell.Background == System.Windows.Media.Brushes.Black ? 1 : 0;
                }
            }
            return input;
        }
        public void zap_f_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.InitialTrainset[0].Item1)); // Отображаем букву A
            MessageBox.Show("Выведена буква 1");
        }

        public void zap_s_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.InitialTrainset[1].Item1)); // Отображаем букву B
            MessageBox.Show("Выведена буква 2");
        }

        public void zap_t_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.InitialTrainset[2].Item1)); // Отображаем букву C
            MessageBox.Show("Выведена буква 3");
        }

        public void zap_fh_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.InitialTrainset[3].Item1)); // Отображаем букву D
            MessageBox.Show("Выведена буква 4");
        }

        public void zap_ff_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.InitialTrainset[4].Item1)); // Отображаем букву E
            MessageBox.Show("Выведена буква 5");
        }

        public void zap_ss_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.FullTrainset[5].Item1)); // Отображаем букву F
            MessageBox.Show("Выведена буква 6");
        }

        public void zap_se_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draw(ConvertTo2DArray(network.input_layer.FullTrainset[6].Item1)); // Отображаем букву G
            MessageBox.Show("Выведена буква 7");
        }
        private int[,] ConvertTo2DArray(double[] input)// Исправить с утра сесть
        {
            int[,] result = new int[7, 7]; // Предполагаем, что размерность 7x7
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    result[i, j] = (int)input[i * 8 + j]; // Преобразуем double в int
                }
            }
            return result;
        }

        public void Draw(int[,] letter)
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Button cell = (Button)grid.Children[i * 7 + j];
                    cell.Background = letter[i, j] == 1 ? cell.Background = System.Windows.Media.Brushes.Black : cell.Background = System.Windows.Media.Brushes.White;
                }
            }
        }

        public void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            RecognizeCurrentLetter();
        }

        public void RecognizeCurrentLetter()
        {
            int[,] currentLetter = Find(); // Получаем текущее состояние нарисованной буквы
            double[] input = InputLayer.ConvertToInput(currentLetter); // Преобразуем в одномерный массив

            // Здесь вы можете использовать вашу нейронную сеть для распознавания
            // Например, если у вас есть метод Recognize в классе Network:
            double[] output = network.Recognize(input); // Предполагается, что у вас есть метод Recognize

            // Определяем букву с максимальным выходом
            int predictedIndex = Array.IndexOf(output, output.Max());
            string predictedLetter = predictedIndex switch
            {
                0 => "1",
                1 => "2",
                2 => "3",
                3 => "4",
                4 => "5",
                5 => "6",
                6 => "7",
                _ => "Unknown"
            };

            MessageBox.Show($"Распознана буква: {predictedLetter}");
        }

        public int[] Flatten(int[,] matrix)
        {
            int[] flatArray = new int[64];
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    flatArray[i * 7 + j] = matrix[i, j];
                }
            }
            return flatArray;
        }

        public void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(inputNeuronsTextBox.Text, out int generations))
            {
                MessageBox.Show("Некорректное значение количества поколений.");
                return;
            }

            if (!int.TryParse(iterationsTextBox.Text, out int populationSize))
            {
                MessageBox.Show("Некорректное значение популяции.");
                return;
            }

            if (!double.TryParse(learningRateTextBox.Text, out double mutationRate))
            {
                MessageBox.Show("Некорректное значение возможности мутации.");
                return;
            }

            network.First(plotModel, generations, populationSize, mutationRate);

            MessageBox.Show("Обучение завершено!");
        }
        public void DoTrainButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(inputNeuronsTextBox.Text, out int inputNeurons))
            {
                MessageBox.Show("Некорректное значение количества входных нейронов.");
                return;
            }

            if (!int.TryParse(iterationsTextBox.Text, out int iterations))
            {
                MessageBox.Show("Некорректное значение количества итераций.");
                return;
            }

            if (!double.TryParse(learningRateTextBox.Text, out double learningRate))
            {
                MessageBox.Show("Некорректное значение скорости обучения.");
                return;
            }

            network.Second(plotModel);

            // Сообщение о завершении обучения
            MessageBox.Show("Обучение завершено!");
        }

        public class Individual
        {
            public double[] Weights { get; set; }
            public double Fitness { get; set; }

            public Individual(int weightCount)
            {
                Weights = new double[weightCount];
                Random rand = new Random();
                for (int i = 0; i < weightCount; i++)
                {
                    Weights[i] = rand.NextDouble() * 2 - 1; // Инициализация весов случайными значениями от -1 до 1
                }
            }
        }

        public class GeneticAlgorithm
        {
            public List<Individual> population;
            private int populationSize;
            private double mutationRate;
            private int weightCount;

            public GeneticAlgorithm(int populationSize, double mutationRate, int weightCount)
            {
                this.populationSize = populationSize;
                this.mutationRate = mutationRate;
                this.weightCount = weightCount;
                population = new List<Individual>();

                // Инициализация популяции
                for (int i = 0; i < populationSize; i++)
                {
                    population.Add(new Individual(weightCount));
                }
            }

            public void EvaluateFitness(Network network, (double[], double[])[] trainset)
            {
                foreach (var individual in population)
                {
                    // Установка весов нейронной сети
                    SetNetworkWeights(network, individual.Weights);

                    // Оценка производительности на обучающем наборе
                    double totalError = 0;
                    foreach (var data in trainset)
                    {
                        double[] output = network.Recognize(data.Item1);
                        double[] errors = new double[data.Item2.Length];
                        for (int j = 0; j < errors.Length; j++)
                        {
                            errors[j] = data.Item2[j] - output[j];
                        }
                        totalError += network.GetMSE(errors);
                    }
                    individual.Fitness = 1.0 / (1.0 + totalError); // Функция приспособленности
                }
            }

            public void SetNetworkWeights(Network network, double[] weights)
            {
                // Здесь нужно установить веса для каждого нейрона в сети
                // Например, если у вас есть доступ к весам скрытого и выходного слоев
                int index = 0;
                for (int i = 0; i < network.hidden_layer.Neurons.Length; i++)
                {
                    for (int j = 0; j < network.hidden_layer.Neurons[i].Weights.Length; j++)
                    {
                        network.hidden_layer.Neurons[i].Weights[j] = weights[index++];
                    }
                }
                for (int i = 0; i < network.output_layer.Neurons.Length; i++)
                {
                    for (int j = 0; j < network.output_layer.Neurons[i].Weights.Length; j++)
                    {
                        network.output_layer.Neurons[i].Weights[j] = weights[index++];
                    }
                }
            }

            public void Evolve()
            {
                // Селекция, скрещивание и мутация
                List<Individual> newPopulation = new List<Individual>();

                // Селекция (например, отбор лучших особей)
                population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness)); // Сортировка по фитнесу
                int eliteCount = populationSize / 10; // Сохраняем 10% лучших особей
                newPopulation.AddRange(population.Take(eliteCount));

                // Скрещивание
                while (newPopulation.Count < populationSize)
                {
                    Individual parent1 = SelectParent();
                    Individual parent2 = SelectParent();
                    Individual child = Crossover(parent1, parent2);
                    Mutate(child);
                    newPopulation.Add(child);
                }

                population = newPopulation;
            }

            private Individual SelectParent()
            {
                // Простой метод рулетки для выбора родителя
                double totalFitness = population.Sum(ind => ind.Fitness);
                double randomValue = new Random().NextDouble() * totalFitness;
                double runningTotal = 0;

                foreach (var individual in population)
                {
                    runningTotal += individual.Fitness;
                    if (runningTotal >= randomValue)
                    {
                        return individual;
                    }
                }
                return population.Last(); // На всякий случай
            }

            private Individual Crossover(Individual parent1, Individual parent2)
            {
                Individual child = new Individual(parent1.Weights.Length);
                for (int i = 0; i < child.Weights.Length; i++)
                {
                    child.Weights[i] = (i % 2 == 0) ? parent1.Weights[i] : parent2.Weights[i];
                }
                return child;
            }

            private void Mutate(Individual individual)
            {
                Random rand = new Random();
                for (int i = 0; i < individual.Weights.Length; i++)
                {
                    if (rand.NextDouble() < mutationRate)
                    {
                        individual.Weights[i] += (rand.NextDouble() * 2 - 1) * 0.1; // Небольшая мутация
                    }
                }
            }
        }

        public class InputLayer
        {
            public int number_out = 7;
            // Начальный обучающий набор данных (5 буквы)
            public (double[], double[])[] InitialTrainset = new (double[], double[])[]
            {
            (ConvertToInput(letterA), new double[] { 1, 0, 0, 0, 0, 0, 0 }), // A
            (ConvertToInput(letterB), new double[] { 0, 1, 0, 0, 0, 0, 0 }), // B
            (ConvertToInput(letterC), new double[] { 0, 0, 1, 0, 0, 0, 0 }), // C
            (ConvertToInput(letterD), new double[] { 0, 0, 0, 1, 0, 0, 0 }), // D
            (ConvertToInput(letterE), new double[] { 0, 0, 0, 0, 1, 0, 0 }) // E
            };

            // Обновленный обучающий набор данных (7 букв)
            public (double[], double[])[] FullTrainset = new (double[], double[])[]
            {
            (ConvertToInput(letterA), new double[] { 1, 0, 0, 0, 0, 0, 0 }), // A
            (ConvertToInput(letterB), new double[] { 0, 1, 0, 0, 0, 0, 0 }), // B
            (ConvertToInput(letterC), new double[] { 0, 0, 1, 0, 0, 0, 0 }), // C
            (ConvertToInput(letterD), new double[] { 0, 0, 0, 1, 0, 0, 0 }), // D
            (ConvertToInput(letterE), new double[] { 0, 0, 0, 0, 1, 0, 0 }), // E
            (ConvertToInput(letterF), new double[] { 0, 0, 0, 0, 0, 1, 0 }), // F
            (ConvertToInput(letterG), new double[] { 0, 0, 0, 0, 0, 0, 1 })  // G
            };
            public void UpdateLetter(int index, int[,] newLetter)
            {
                if (index < 0 || index >= number_out)
                    throw new ArgumentOutOfRangeException(nameof(index));

                // Обновляем букву в InitialTrainset
                if (index < 5)
                InitialTrainset[index] = (ConvertToInputE(newLetter), InitialTrainset[index].Item2);
                // Обновляем букву в FullTrainset
                FullTrainset[index] = (ConvertToInputE(newLetter), FullTrainset[index].Item2);
            }

            // Преобразование двумерного массива в одномерный
            public static double[] ConvertToInput(int[,] letter)
            {
                int rows = letter.GetLength(0);
                int cols = letter.GetLength(1);
                double[] input = new double[rows * cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        input[i * cols + j] = letter[i, j];
                    }
                }

                return input;
            }
            public static double[] ConvertToInputE(int[,] letter)
            {
                int rows = letter.GetLength(0);
                int cols = letter.GetLength(1);
                double[] input = new double[(rows) * (cols)];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        input[i * cols + j] = letter[i, j];
                    }
                }

                return input;
            }

            // Определение букв
            public static int[,] letterA = {
            {0, 0, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 1, 1, 1, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterB = {
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterC = {
            {0, 0, 1, 1, 1, 1, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 0, 1, 1, 1, 1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterD = {
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterE = {
            {0, 1, 1, 1, 1, 1, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 1, 1, 1, 1, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 1, 1, 1, 1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterF = {
            {0, 1, 1, 1, 1, 1, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

            public static int[,] letterG = {
            {0, 0, 1, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 1, 1, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 0, 0, 1, 0, 0},
            {0, 0, 1, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };
        }

        public class Neuron
        {
            public Neuron(double[] inputs, double[] weights, NeuronType type)
            {
                _type = type;
                _weights = weights;
                _inputs = inputs;
            }
            public NeuronType _type;
            public double[] _weights;
            public double[] _inputs;
            public double[] Weights { get => _weights; set => _weights = value; }
            public double[] Inputs { get => _inputs; set => _inputs = value; }
            public double Output { get => Activator(_inputs, _weights); }
            public double Activator(double[] i, double[] w)
            {
                double sum = 0;
                for (int l = 0; l < i.Length; ++l)
                    sum += i[l] * w[l];
                return 1 / (1 + Exp(-sum)); // Сигмоидная функция
            }
            public double Derivativator(double outsignal) => outsignal * (1 - outsignal);
            public double Gradientor(double error, double dif, double g_sum) => (_type == NeuronType.Output) ? error * dif : g_sum * dif;
        }

        public abstract class Layer
        {
            public Layer(int non, int nopn, NeuronType nt, string type)
            {
                numofneurons = non;
                numofprevneurons = nopn;
                Neurons = new Neuron[non];
                double[,] Weights = WeightInitialize(MemoryMode.GET, type);
                for (int i = 0; i < non; ++i)
                {
                    double[] temp_weights = new double[nopn];
                    for (int j = 0; j < nopn; ++j)
                        temp_weights[j] = Weights[i, j];
                    Neurons[i] = new Neuron(null, temp_weights, nt);
                }
            }

            public int numofneurons;
            public int numofprevneurons;
            public const double learningrate = 0.01d; // Уменьшенная скорость обучения
            Neuron[] _neurons;
            public Neuron[] Neurons { get => _neurons; set => _neurons = value; }
            public double[] Data
            {
                set
                {
                    for (int i = 0; i < Neurons.Length; ++i)
                        Neurons[i].Inputs = value;
                }
            }
            public double[,] WeightInitialize(MemoryMode mm, string type)
            {
                double[,] _weights = new double[numofneurons, numofprevneurons];
                Console.WriteLine($"{type} weights are being initialized...");
                Random rand = new Random();
                for (int l = 0; l < _weights.GetLength(0); ++l)
                    for (int k = 0; k < _weights.GetLength(1); ++k)
                        _weights[l, k] = rand.NextDouble() * 2 - 1; // Инициализация случайными значениями от -1 до 1
                Console.WriteLine($"{type} weights have been initialized...");
                return _weights;
            }
            abstract public void Recognize(Network net, Layer nextLayer);
            abstract public double[] BackwardPass(double[] stuff);
        }

        public class HiddenLayer : Layer
        {
            public HiddenLayer(int non, int nopn, NeuronType nt, string type) : base(non, nopn, nt, type) { }
            public override void Recognize(Network net, Layer nextLayer)
            {
                double[] hidden_out = new double[Neurons.Length];
                for (int i = 0; i < Neurons.Length; ++i)
                    hidden_out[i] = Neurons[i].Output;
                nextLayer.Data = hidden_out;
            }
            public override double[] BackwardPass(double[] gr_sums)
            {
                for (int i = 0; i < numofneurons; ++i)
                    for (int n = 0; n < numofprevneurons; ++n)
                        Neurons[i].Weights[n] += learningrate * Neurons[i].Inputs[n] * Neurons[i].Gradientor(0, Neurons[i].Derivativator(Neurons[i].Output), gr_sums[i]);
                return null;
            }
        }

        public class OutputLayer : Layer
        {
            public OutputLayer(int non, int nopn, NeuronType nt, string type) : base(non, nopn, nt, type) { }
            public override void Recognize(Network net, Layer nextLayer)
            {
                for (int i = 0; i < Neurons.Length; ++i)
                    net.fact[i] = Neurons[i].Output;
            }
            public override double[] BackwardPass(double[] errors)
            {
                double[] gr_sum = new double[numofprevneurons];
                for (int j = 0; j < gr_sum.Length; ++j)
                {
                    double sum = 0;
                    for (int k = 0; k < Neurons.Length; ++k)
                        sum += Neurons[k].Weights[j] * Neurons[k].Gradientor(errors[k], Neurons[k].Derivativator(Neurons[k].Output), 0);
                    gr_sum[j] = sum;
                }
                for (int i = 0; i < numofneurons; ++i)
                    for (int n = 0; n < numofprevneurons; ++n)
                        Neurons[i].Weights[n] += learningrate * Neurons[i].Inputs[n] * Neurons[i].Gradientor(errors[i], Neurons[i].Derivativator(Neurons[i].Output), 0);
                return gr_sum;
            }
        }

        public class Network
        {
            public InputLayer input_layer = new InputLayer();
            public HiddenLayer hidden_layer = new HiddenLayer(32, 64, NeuronType.Hidden, nameof(hidden_layer));
            public OutputLayer output_layer = new OutputLayer(number_out, 32, NeuronType.Output, nameof(output_layer));
            public double[] fact = new double[number_out];
            public static int number_out = 7;

            public double GetMSE(double[] errors)
            {
                double sum = 0;
                for (int i = 0; i < errors.Length; ++i)
                    sum += Pow(errors[i], 2);
                return 0.5d * sum;
            }

            double GetCost(double[] mses)
            {
                double sum = 0;
                for (int i = 0; i < mses.Length; ++i)
                    sum += mses[i];
                return (sum / mses.Length);
            }
            /*public void TrainWithGeneticAlgorithm(int generations, int populationSize, double mutationRate)
            {
                int weightCount = hidden_layer.Neurons.Length * hidden_layer.Neurons[0].Weights.Length +
                                  output_layer.Neurons.Length * output_layer.Neurons[0].Weights.Length;

                GeneticAlgorithm ga = new GeneticAlgorithm(populationSize, mutationRate, weightCount);

                for (int generation = 0; generation < generations; generation++)
                {
                    ga.EvaluateFitness(this, input_layer.FullTrainset);
                    ga.Evolve();
                    Console.WriteLine($"Generation {generation} completed.");
                }

                // Получение лучшей особи после всех поколений
                var bestIndividual = ga.population.OrderByDescending(ind => ind.Fitness).First();
                ga.SetNetworkWeights(this, bestIndividual.Weights);
            }*/
            public void TrainWithGeneticAlgorithm(int generations, int populationSize, double mutationRate, PlotModelViewModel plotModel)
            {
                int weightCount = hidden_layer.Neurons.Length * hidden_layer.Neurons[0].Weights.Length +
                                  output_layer.Neurons.Length * output_layer.Neurons[0].Weights.Length;

                GeneticAlgorithm ga = new GeneticAlgorithm(populationSize, mutationRate, weightCount);

                // Списки для хранения данных
                List<double> learningRates = new List<double>();
                List<double> populationSizes = new List<double>();
                List<double> performanceMetrics = new List<double>();
                List<double> generationCount = new List<double>();

                for (int generation = 0; generation < generations; generation++)
                {
                    ga.EvaluateFitness(this, input_layer.FullTrainset);
                    ga.Evolve();

                    // Сохраняем данные для графика
                    learningRates.Add(mutationRate); // Здесь можно использовать другую логику для изменения скорости обучения
                    populationSizes.Add(populationSize);
                    performanceMetrics.Add(ga.population.Max(ind => ind.Fitness)); // Максимальная приспособленность
                    generationCount.Add(generation);

                    Console.WriteLine($"Generation {generation} completed.");
                }

                // Получение лучшей особи после всех поколений
                var bestIndividual = ga.population.OrderByDescending(ind => ind.Fitness).First();
                SetNetworkWeights(bestIndividual.Weights);

                // Построение графика
                for (int i = 0; i < learningRates.Count; i++)
                {
                    plotModel.AddPoint(generationCount[i], performanceMetrics[i]);
                }
            }
            public void SetNetworkWeights(double[] weights)
            {
                int index = 0;

                // Установка весов для скрытого слоя
                for (int i = 0; i < hidden_layer.Neurons.Length; i++)
                {
                    for (int j = 0; j < hidden_layer.Neurons[i].Weights.Length; j++)
                    {
                        hidden_layer.Neurons[i].Weights[j] = weights[index++];
                    }
                }

                // Установка весов для выходного слоя
                for (int i = 0; i < output_layer.Neurons.Length; i++)
                {
                    for (int j = 0; j < output_layer.Neurons[i].Weights.Length; j++)
                    {
                        output_layer.Neurons[i].Weights[j] = weights[index++];
                    }
                }
            }
            static void Train(Network net, (double[], double[])[] trainset, PlotModelViewModel plotModel)
            {
                const double threshold = 0.01d;
                double[] temp_mses = new double[number_out];
                double temp_cost = 0;
                int iteration = 0;
                do
                {
                    for (int i = 0; i < trainset.Length; ++i)
                    {
                        net.hidden_layer.Data = trainset[i].Item1;
                        net.hidden_layer.Recognize(null, net.output_layer);
                        net.output_layer.Recognize(net, null);

                        double[] errors = new double[trainset[i].Item2.Length];
                        for (int x = 0; x < errors.Length; ++x)
                            errors[x] = trainset[i].Item2[x] - net.fact[x];

                        temp_mses[i] = net.GetMSE(errors);
                        double[] temp_gsums = net.output_layer.BackwardPass(errors);
                        net.hidden_layer.BackwardPass(temp_gsums);
                    }
                    temp_cost = net.GetCost(temp_mses);
                    Console.WriteLine($"Current cost: {temp_cost}");
                    plotModel.AddPoint(iteration++, temp_cost);
                } while (temp_cost > threshold);
            }
            public void Test()
            {
                // Тестирование на всех шести буквах
                foreach (var data in input_layer.FullTrainset)
                {
                    hidden_layer.Data = data.Item1;
                    hidden_layer.Recognize(null, output_layer);
                    output_layer.Recognize(this, null);

                    // Определяем букву с максимальным выходом
                    int predictedIndex = Array.IndexOf(fact, fact.Max());
                    string predictedLetter = predictedIndex switch
                    {
                        0 => "1",
                        1 => "2",
                        2 => "3",
                        3 => "4",
                        4 => "5",
                        5 => "6",
                        6 => "7",
                        _ => "Unknown"
                    };

                    // Выводим входные данные и вероятности
                    Console.WriteLine($"Output for input {string.Join(", ", data.Item1)}: {string.Join(", ", fact.Select(f => f.ToString("F2")))}");
                    Console.WriteLine($"Predicted letter: {predictedLetter}");
                }
            }
            public double[] Recognize(double[] input)
            {
                // Здесь должна быть логика распознавания
                // Например, вы можете передать входные данные через скрытый и выходной слои
                hidden_layer.Data = input;
                hidden_layer.Recognize(this, output_layer);
                output_layer.Recognize(this, null);

                return fact; // Возвращаем выходные данные
            }

            public void First(PlotModelViewModel plotModel, int generations, int populationSize, double mutationRate)
            {
                plotModel.Clear();
                Console.WriteLine("Network parameters:");
                Console.WriteLine($"Learning rate: {Layer.learningrate}");
                Console.WriteLine($"Number of neurons in hidden layer: {hidden_layer.Neurons.Length}");
                Console.WriteLine($"Number of neurons in output layer: {output_layer.Neurons.Length}");
                Console.WriteLine($"Number of input features: {input_layer.InitialTrainset[0].Item1.Length}");

                TrainWithGeneticAlgorithm(generations, populationSize, mutationRate, plotModel);
                Test();
            }

            public void Second(PlotModelViewModel plotModel)
            {
                plotModel.Clear();
                Console.WriteLine("Network parameters:");
                Console.WriteLine($"Learning rate: {Layer.learningrate}");
                Console.WriteLine($"Number of neurons in hidden layer: {hidden_layer.Neurons.Length}");
                Console.WriteLine($"Number of neurons in output layer: {output_layer.Neurons.Length}");
                Console.WriteLine($"Number of input features: {input_layer.InitialTrainset[0].Item1.Length}");

                // Дообучение на 7 буквах с использованием генетического алгоритма
                int generations = 500; // Количество поколений
                int populationSize = 200; // Размер популяции
                double mutationRate = 0.01; // Вероятность мутации

                TrainWithGeneticAlgorithm(generations, populationSize, mutationRate, plotModel);
                Test(); // Тестирование после дообучения
            }
        }
    }
    public class PlotModelViewModel
    {
        public PlotModel Model { get; private set; }
        public LineSeries Series { get; private set; }

        public PlotModelViewModel()
        {
            Model = new PlotModel { Title = "Training Cost" };
            Series = new LineSeries { Title = "Cost", Color = OxyColors.Black, StrokeThickness = 2 };
            Model.Series.Add(Series);

            // Установите диапазоны осей
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Iteration" });
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Cost", Minimum = 0 }); // Установите минимальное значение
        }

        public void AddPoint(double x, double y)
        {
            Series.Points.Add(new DataPoint(x, y));
            Console.WriteLine($"Added point: ({x}, {y})"); // Отладочное сообщение
            Model.InvalidatePlot(true);
        }
        public void Clear()
        {
            Series.Points.Clear(); // Очищаем все точки
            Model.InvalidatePlot(true); // Обновляем график
        }
    }
}