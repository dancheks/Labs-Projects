using MahApps.Metro.Controls;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlliteViewer.Views;

namespace SqlliteViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ProductsView _productsView;
        private RecipientsView _recipientsView;
        private StoragesView _storagesView;
        private StoredView _storedView;
        private Transfer_StatusView _transferStatusView;
        private StatisticsView _statisticsView;

        private void ShowProducts(object sender, RoutedEventArgs e)
        {
            _productsView ??= new ProductsView();
            ContentPresenter.Content = _productsView;
            first.Background = new SolidColorBrush(Colors.Aquamarine);
            second.Background = new SolidColorBrush(Colors.Transparent);
            third.Background = new SolidColorBrush(Colors.Transparent);
            forth.Background = new SolidColorBrush(Colors.Transparent);
            fifth.Background = new SolidColorBrush(Colors.Transparent);
            sixth.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowRecipients(object sender, RoutedEventArgs e)
        {
            _recipientsView ??= new RecipientsView();
            ContentPresenter.Content = _recipientsView;
            first.Background = new SolidColorBrush(Colors.Transparent);
            second.Background = new SolidColorBrush(Colors.Aquamarine);
            third.Background = new SolidColorBrush(Colors.Transparent);
            forth.Background = new SolidColorBrush(Colors.Transparent);
            fifth.Background = new SolidColorBrush(Colors.Transparent);
            sixth.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowStorages(object sender, RoutedEventArgs e)
        {
            _storagesView ??= new StoragesView();
            ContentPresenter.Content = _storagesView;
            first.Background = new SolidColorBrush(Colors.Transparent);
            second.Background = new SolidColorBrush(Colors.Transparent);
            third.Background = new SolidColorBrush(Colors.Aquamarine);
            forth.Background = new SolidColorBrush(Colors.Transparent);
            fifth.Background = new SolidColorBrush(Colors.Transparent);
            sixth.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowStored(object sender, RoutedEventArgs e)
        {
            _storedView ??= new StoredView();
            ContentPresenter.Content = _storedView;
            first.Background = new SolidColorBrush(Colors.Transparent);
            second.Background = new SolidColorBrush(Colors.Transparent);
            third.Background = new SolidColorBrush(Colors.Transparent);
            forth.Background = new SolidColorBrush(Colors.Aquamarine);
            fifth.Background = new SolidColorBrush(Colors.Transparent);
            sixth.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowTransferStatus(object sender, RoutedEventArgs e)
        {
            _transferStatusView ??= new Transfer_StatusView();
            ContentPresenter.Content = _transferStatusView;
            first.Background = new SolidColorBrush(Colors.Transparent);
            second.Background = new SolidColorBrush(Colors.Transparent);
            third.Background = new SolidColorBrush(Colors.Transparent);
            forth.Background = new SolidColorBrush(Colors.Transparent);
            fifth.Background = new SolidColorBrush(Colors.Aquamarine);
            sixth.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowStatistics(object sender, RoutedEventArgs e)
        {
            _statisticsView ??= new StatisticsView();
            ContentPresenter.Content = _statisticsView;
            first.Background = new SolidColorBrush(Colors.Transparent);
            second.Background = new SolidColorBrush(Colors.Transparent);
            third.Background = new SolidColorBrush(Colors.Transparent);
            forth.Background = new SolidColorBrush(Colors.Transparent);
            fifth.Background = new SolidColorBrush(Colors.Transparent);
            sixth.Background = new SolidColorBrush(Colors.Aquamarine);
        }
    }
}