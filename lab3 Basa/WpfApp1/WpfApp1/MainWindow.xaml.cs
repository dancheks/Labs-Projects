using System;
using System.Windows;

namespace CarManagerApp
{
    public partial class MainWindow : Window
    {
        private CarManager carManager = new CarManager();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var car = new Car
            {
                Brand = BrandTextBox.Text,
                Model = ModelTextBox.Text,
                Year = int.Parse(YearTextBox.Text),
                Mileage = int.Parse(MileageTextBox.Text)
            };
            carManager.AddCar(car);
            UpdateCarList();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarsDataGrid.SelectedItem is Car selectedCar)
            {
                carManager.RemoveCar(selectedCar);
                UpdateCarList();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var brand = BrandTextBox.Text.Trim();
            var model = ModelTextBox.Text.Trim();

            // Получаем все автомобили
            var allCars = carManager.GetAllCars();

            // Фильтруем автомобили по введенным данным
            var results = allCars.FindAll(c =>
                (string.IsNullOrEmpty(brand) || c.Brand.IndexOf(brand, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrEmpty(model) || c.Model.IndexOf(model, StringComparison.OrdinalIgnoreCase) >= 0));

            // Обновляем DataGrid с результатами поиска
            CarsDataGrid.ItemsSource = results;
        }

        private void SaveBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            carManager.SaveToBinary("cars.dat");
        }

        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            carManager.SaveToXml("cars.xml");
        }

        private void LoadBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            var cars = carManager.LoadFromBinary("cars.dat");
            foreach (var car in cars)
            {
                carManager.AddCar(car);
            }
            UpdateCarList();
        }

        private void LoadXmlButton_Click(object sender, RoutedEventArgs e)
        {
            var cars = carManager.LoadFromXml("cars.xml");
            foreach (var car in cars)
            {
                carManager.AddCar(car);
            }
            UpdateCarList();
        }

        private void UpdateCarList()
        {
            CarsDataGrid.ItemsSource = null;
            CarsDataGrid.ItemsSource = carManager.GetAllCars();
        }
    }
}