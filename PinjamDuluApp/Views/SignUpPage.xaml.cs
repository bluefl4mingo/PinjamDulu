using PinjamDuluApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PinjamDuluApp.Views
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        private SignUpViewModel _viewModel;
        public SignUpPage()
        {
            InitializeComponent();
            _viewModel = new SignUpViewModel(MainWindow.NavigationService);
            DataContext = _viewModel;

            signBoxPassword.PasswordChanged += (s, e) =>
            {
                _viewModel.Password = signBoxPassword.Password;
            };

            signBoxConfPassword.PasswordChanged += (s, e) =>
            {
                _viewModel.ConfirmPassword = signBoxConfPassword.Password;
            };
        }       

        private void signTextEmail_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            signBoxEmail.Focus();
        }

        private void signTextPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            signBoxPassword.Focus();
        }

        private void signTextConfPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            signBoxConfPassword.Focus();
        }

        private void signBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(signBoxEmail.Text) && signBoxEmail.Text.Length > 0)
            {
                signTextEmail.Visibility = Visibility.Collapsed;
            }
            else
            {
                signTextEmail.Visibility = Visibility.Visible;
            }
        }

        private void signBoxPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(signBoxPassword.Password) && signBoxPassword.Password.Length > 0)
            {
                signTextPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                signTextPassword.Visibility = Visibility.Visible;
            }
        }

        private void signBoxConfPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(signBoxConfPassword.Password) && signBoxConfPassword.Password.Length > 0)
            {
                signTextConfPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                signTextConfPassword.Visibility = Visibility.Visible;
            }
        }
    }
}
