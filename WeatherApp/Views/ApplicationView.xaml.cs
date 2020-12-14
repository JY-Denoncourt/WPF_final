using OpenWeatherAPI;
using System.Windows;
using WeatherApp.Services;
using WeatherApp.ViewModels;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ApplicationView : Window
    {
        ApplicationViewModel vm;

        public ApplicationView()
        {
            InitializeComponent();

            ApiHelper.InitializeClient();

            vm = new ApplicationViewModel();

            DataContext = vm;            
        }
    }
}
