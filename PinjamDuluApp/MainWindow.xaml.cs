using PinjamDuluApp.Views;
using System.Windows;
using NavigationService = PinjamDuluApp.Services.NavigationService;

namespace PinjamDuluApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NavigationService NavigationService { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            NavigationService = new NavigationService(MainFrame);
            NavigationService.NavigateTo(typeof(LoginPage)); // first page that shows up when application starts 
        }
    }
}