using PinjamDuluApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PinjamDuluApp.Views
{
    public partial class LoginPage : Page
    {
        private LoginViewModel _viewModel;
        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel(MainWindow.NavigationService); // Every ViewModel's constructor receives NavigationServices to transition between pages
            DataContext = _viewModel;
        }

        private void logTextEmail_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logBoxEmail.Focus();
        }

        private void logTextPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logBoxPassword.Focus();
        }

        private void logBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(logBoxEmail.Text) && logBoxEmail.Text.Length > 0)
            {
                logTextEmail.Visibility = Visibility.Collapsed;
            }
            else
            {
                logTextEmail.Visibility = Visibility.Visible;
            }
        }

        private void logBoxPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(logBoxPassword.Password) && logBoxPassword.Password.Length > 0)
            {
                logTextPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                logTextPassword.Visibility = Visibility.Visible;
            }
        }
    }
}
