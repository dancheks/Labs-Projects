using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BranchAndBoundVisualizer
{
    public partial class MainWindow : Window
    {
        private BranchAndBoundSolver solver;
        private List<VisualizationNode> visualizationNodes = new List<VisualizationNode>();
        private bool isRunning = false;
        private bool stepMode = false;
        private int animationDelay = 500;
        private DispatcherTimer animationTimer;
        private double baseNodeSize = 60;
private double minNodeSize = 20;
private double currentZoom = 1.0;
private double baseLevelHeight = 100;
        public MainWindow()
        {
            InitializeComponent();
            InitializeCostMatrix();
            animationTimer = new DispatcherTimer();
            animationTimer.Tick += AnimationTimer_Tick;
            this.SizeChanged += MainWindow_SizeChanged;
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (visualizationNodes.Any())
            {
                CalculateNodePositions();
                DrawTree();
            }
        }

        private void InitializeCostMatrix()
        {
            int INF = int.MaxValue;
            int[,] defaultMatrix = {
            { INF, 11, 10, 12, 6, 7, 8 },
            { 16, INF, 5, 6, 11, 8, 8 },
            { 11, 10, INF, 11, 5, 6, 7 },
            { 6, 8, 13, INF, 7, 6, 8 },
            { 5, 7, 8, 9, INF, 5, 7 },
            { 14, 6, 7, 14, 11, INF, 6 },
            { 6, 15, 6, 8, 10, 7, INF}
            };

        CostMatrixGrid.Columns.Clear();
            CostMatrixGrid.Columns.Add(new DataGridTextColumn() { Header = "From/To", Binding = new System.Windows.Data.Binding("RowHeader"), IsReadOnly = true });

            for (int i = 0; i < defaultMatrix.GetLength(1); i++)
            {
                CostMatrixGrid.Columns.Add(new DataGridTextColumn() { Header = $"To {i + 1}", Binding = new System.Windows.Data.Binding($"Col{i}") });
            }

            CostMatrixGrid.Items.Clear();
            for (int i = 0; i < defaultMatrix.GetLength(0); i++)
            {
                dynamic item = new System.Dynamic.ExpandoObject();
                item.RowHeader = $"From {i + 1}";
                for (int j = 0; j < defaultMatrix.GetLength(1); j++)
                {
                    ((IDictionary<string, object>)item)["Col" + j] = defaultMatrix[i, j] == INF ? "∞" : defaultMatrix[i, j].ToString();
                }
                CostMatrixGrid.Items.Add(item);
            }
        }

        private int[,] GetCostMatrixFromUI()
        {
            int size = CostMatrixGrid.Items.Count;
            int[,] matrix = new int[size, size];
            int INF = int.MaxValue;

            for (int i = 0; i < size; i++)
            {
                dynamic item = CostMatrixGrid.Items[i];
                for (int j = 0; j < size; j++)
                {
                    string value = ((IDictionary<string, object>)item)["Col" + j].ToString();
                    matrix[i, j] = value == "∞" ? INF : int.Parse(value);
                }
            }

            return matrix;
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                RunButton.Content = "Run Algorithm";
                StepButton.IsEnabled = false;
                return;
            }

            TreeCanvas.Children.Clear();
            visualizationNodes.Clear();
            LogText.Text = "";
            BestSolutionText.Text = "";
            CurrentPathText.Text = "";

            int[,] costMatrix = GetCostMatrixFromUI();
            solver = new BranchAndBoundSolver(costMatrix);

            RunButton.Content = "Stop";
            StepButton.IsEnabled = true;
            isRunning = true;
            stepMode = false;

            await Task.Run(() => solver.Solve(UpdateVisualization, UpdateBestSolution));

            isRunning = false;
            RunButton.Content = "Run Algorithm";
            StepButton.IsEnabled = false;
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                TreeCanvas.Children.Clear();
                visualizationNodes.Clear();
                LogText.Text = "";
                BestSolutionText.Text = "";
                CurrentPathText.Text = "";

                int[,] costMatrix = GetCostMatrixFromUI();
                solver = new BranchAndBoundSolver(costMatrix);

                RunButton.Content = "Stop";
                StepButton.Content = "Next Step";
                isRunning = true;
                stepMode = true;

                // Start the algorithm in step mode
                animationTimer.Interval = TimeSpan.FromMilliseconds(1);
                animationTimer.Start();
            }
            else if (stepMode)
            {
                // Trigger next step
                solver.ContinueExecution();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationTimer.Stop();
            if (isRunning && stepMode)
            {
                solver.ContinueExecution();
            }
        }

        private void UpdateVisualization(List<int> currentPath, int currentCost, int bound, string action)
        {
            Dispatcher.Invoke(() =>
            {
                string pathString = string.Join(" → ", currentPath.Select(p => p + 1));
                if (currentPath.Count > 1)
                {
                    pathString += $" → {currentPath[0] + 1}";
                }

                CurrentPathText.Text = $"Path: {pathString}\nCurrent Cost: {currentCost}\nBound: {bound}";

                string logEntry = $"{action}: {pathString} (Cost: {currentCost}, Bound: {bound})";
                LogText.Text = logEntry + "\n" + LogText.Text;

                AddNodeToVisualization(currentPath, currentCost, bound, action);
            });

            if (!stepMode)
            {
                System.Threading.Thread.Sleep(animationDelay);
            }
        }

        private void UpdateBestSolution(List<int> bestPath, int bestCost)
        {
            Dispatcher.Invoke(() =>
            {
                string pathString = string.Join(" → ", bestPath.Select(p => p + 1));
                BestSolutionText.Text = $"Best Path: {pathString}\nBest Cost: {bestCost}";
            });
        }

        private void AddNodeToVisualization(List<int> path, int cost, int bound, string action)
        {
            double nodeSize = 60;
            double levelHeight = 100;
            double horizontalSpacing = 30;

            // Create the node
            var node = new VisualizationNode
            {
                Level = path.Count - 1,
                Path = new List<int>(path),
                Cost = cost,
                Bound = bound,
                Action = action,
                Width = nodeSize + horizontalSpacing
            };

            // Find parent node if exists
            if (node.Level > 0)
            {
                var parentPath = path.Take(path.Count - 1).ToList();
                var parentNode = visualizationNodes.FirstOrDefault(n => n.Level == node.Level - 1 && n.Path.SequenceEqual(parentPath));
                if (parentNode != null)
                {
                    node.Parent = parentNode;
                    parentNode.Children.Add(node);
                }
            }

            visualizationNodes.Add(node);

            // Calculate positions after building the tree structure
            CalculateNodePositions();

            // Now draw all nodes with updated positions
            DrawTree();
        }

        private void CalculateNodePositions()
        {
            if (!visualizationNodes.Any()) return;

            // Start with root node
            var root = visualizationNodes.First(n => n.Level == 0);
            CalculateSubtreeWidth(root);
            CalculateFinalPositions(root, TreeCanvas.ActualWidth / 2 - root.Width / 2);
        }

        private double CalculateSubtreeWidth(VisualizationNode node)
        {
            if (!node.Children.Any())
            {
                node.Width = 60; // Just the node width
                return node.Width;
            }

            double totalWidth = 0;
            foreach (var child in node.Children)
            {
                totalWidth += CalculateSubtreeWidth(child);
            }

            // Add some spacing between children
            totalWidth += Math.Max(0, node.Children.Count - 1) * 30;
            node.Width = Math.Max(60, totalWidth); // At least node width
            return node.Width;
        }

        private void CalculateFinalPositions(VisualizationNode node, double startX)
        {
            double levelHeight = 100;
            node.X = startX + node.Width / 2;
            node.Y = 20 + node.Level * levelHeight;

            if (!node.Children.Any()) return;

            double currentX = startX;
            foreach (var child in node.Children)
            {
                CalculateFinalPositions(child, currentX);
                currentX += child.Width + 30; // Add spacing
            }
        }

        private void DrawTree()
        {
            TreeCanvas.Children.Clear();

            // First draw all lines so they appear behind nodes
            foreach (var node in visualizationNodes)
            {
                if (node.Parent != null)
                {
                    Line line = new Line
                    {
                        X1 = node.Parent.X,
                        Y1 = node.Parent.Y + 60,
                        X2 = node.X,
                        Y2 = node.Y,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    TreeCanvas.Children.Add(line);
                }
            }

            // Then draw all nodes
            foreach (var node in visualizationNodes)
            {
                DrawNode(node);
            }
        }

        private void DrawNode(VisualizationNode node)
        {
            double nodeSize = 60;

            Ellipse ellipse = new Ellipse
            {
                Width = nodeSize,
                Height = nodeSize,
                Fill = node.Action == "Pruned" ? Brushes.LightCoral :
                      (node.Action == "New best" ? Brushes.LightGreen : Brushes.LightBlue),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            TextBlock textBlock = new TextBlock
            {
                Text = $"{node.Path.Last() + 1}\nC:{node.Cost}\nB:{node.Bound}",
                TextAlignment = TextAlignment.Center,
                FontSize = 10,
                Width = nodeSize,
                TextWrapping = TextWrapping.Wrap
            };

            Canvas.SetLeft(ellipse, node.X - nodeSize / 2);
            Canvas.SetTop(ellipse, node.Y);
            Canvas.SetLeft(textBlock, node.X - nodeSize / 2);
            Canvas.SetTop(textBlock, node.Y + nodeSize / 4);

            TreeCanvas.Children.Add(ellipse);
            TreeCanvas.Children.Add(textBlock);

            string tooltipText = $"Path: {string.Join(" → ", node.Path.Select(p => p + 1))}\n" +
                               $"Cost: {node.Cost}\n" +
                               $"Bound: {node.Bound}\n" +
                               $"Action: {node.Action}";

            ToolTip tooltip = new ToolTip { Content = tooltipText };
            ellipse.ToolTip = tooltip;
            textBlock.ToolTip = tooltip;
        }

        private void AnimationSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            animationDelay = (int)(1000 - e.NewValue);
        }
    }

    public class VisualizationNode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Level { get; set; }
        public List<int> Path { get; set; }
        public int Cost { get; set; }
        public int Bound { get; set; }
        public string Action { get; set; }
        public double Width { get; set; } = 60; // Node width including margins
        public List<VisualizationNode> Children { get; set; } = new List<VisualizationNode>();
        public VisualizationNode Parent { get; set; }
    }

    public class BranchAndBoundSolver
    {
        private int[,] costMatrix;
        private int size;
        private int bestCost = int.MaxValue;
        private List<int> bestPath;
        private Action<List<int>, int, int, string> visualizationCallback;
        private Action<List<int>, int> bestSolutionCallback;
        private bool pauseRequested = false;
        private bool stepExecution = false;

        public BranchAndBoundSolver(int[,] matrix)
        {
            this.size = matrix.GetLength(0);
            this.costMatrix = matrix;
        }

        public (List<int> path, int cost) Solve(Action<List<int>, int, int, string> visualizationCallback = null,
                                              Action<List<int>, int> bestSolutionCallback = null)
        {
            this.visualizationCallback = visualizationCallback;
            this.bestSolutionCallback = bestSolutionCallback;

            List<int> path = new List<int> { 0 };
            bool[] visited = new bool[size];
            visited[0] = true;

            Branch(path, visited, 0, costMatrix, 0);
            return (bestPath, bestCost);
        }

        public void ContinueExecution()
        {
            stepExecution = true;
        }

        private void Branch(List<int> path, bool[] visited, int level, int[,] matrix, int currentCost)
        {
            if (path.Count == size)
            {
                int totalCost = currentCost + costMatrix[path[^1], path[0]];
                if (totalCost < bestCost)
                {
                    bestCost = totalCost;
                    bestPath = new List<int>(path);
                    bestPath.Add(path[0]);
                    visualizationCallback?.Invoke(path, totalCost, totalCost, "New best");
                    bestSolutionCallback?.Invoke(bestPath, bestCost);
                }
                else
                {
                    visualizationCallback?.Invoke(path, totalCost, totalCost, "Complete path");
                }
                return;
            }

            var reducedMatrix = ReduceMatrix(CloneMatrix(matrix), out int reductionCost);
            int bound = currentCost + reductionCost;

            visualizationCallback?.Invoke(path, currentCost, bound, "Exploring");

            if (bound >= bestCost)
            {
                visualizationCallback?.Invoke(path, currentCost, bound, "Pruned");
                return;
            }

            for (int i = 0; i < size; i++)
            {
                if (!visited[i] && matrix[path[^1], i] != int.MaxValue)
                {
                    visited[i] = true;
                    path.Add(i);

                    int[,] newMatrix = CloneMatrix(matrix);
                    for (int k = 0; k < size; k++) newMatrix[path[^2], k] = int.MaxValue;
                    for (int k = 0; k < size; k++) newMatrix[k, i] = int.MaxValue;
                    newMatrix[i, path[0]] = int.MaxValue;

                    Branch(path, visited, level + 1, newMatrix, currentCost + matrix[path[^2], i]);

                    path.RemoveAt(path.Count - 1);
                    visited[i] = false;

                    if (stepExecution)
                    {
                        stepExecution = false;
                        while (!stepExecution && !pauseRequested)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    if (pauseRequested)
                    {
                        return;
                    }
                }
            }
        }

        private int ReduceMatrix(int[,] matrix, out int reductionCost)
        {
            reductionCost = 0;

            // Row reduction
            for (int i = 0; i < size; i++)
            {
                var rowValues = Enumerable.Range(0, size)
                    .Where(j => matrix[i, j] != int.MaxValue)
                    .Select(j => matrix[i, j])
                    .ToList();

                if (rowValues.Any())
                {
                    int rowMin = rowValues.Min();
                    for (int j = 0; j < size; j++)
                        if (matrix[i, j] != int.MaxValue) matrix[i, j] -= rowMin;
                    reductionCost += rowMin;
                }
            }

            // Column reduction
            for (int j = 0; j < size; j++)
            {
                var colValues = Enumerable.Range(0, size)
                    .Where(i => matrix[i, j] != int.MaxValue)
                    .Select(i => matrix[i, j])
                    .ToList();

                if (colValues.Any())
                {
                    int colMin = colValues.Min();
                    for (int i = 0; i < size; i++)
                        if (matrix[i, j] != int.MaxValue) matrix[i, j] -= colMin;
                    reductionCost += colMin;
                }
            }

            return reductionCost;
        }

        private int[,] CloneMatrix(int[,] matrix)
        {
            int[,] copy = new int[size, size];
            Array.Copy(matrix, copy, matrix.Length);
            return copy;
        }
    }
}