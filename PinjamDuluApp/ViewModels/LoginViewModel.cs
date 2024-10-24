using System.Windows.Input;
using PinjamDuluApp.Views;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;

namespace PinjamDuluApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        private string _email;
        private string _password;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }

        public LoginViewModel(NavigationService navigationService)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(async () => await Login(), CanLogin);
            NavigateToSignUpCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(SignUpPage)));
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task Login()
        {
            try
            {
                var user = await _databaseService.AuthenticateUser(Email, Password);
                if (user != null)
                {
                    // TODO: Store user session
                    _navigationService.NavigateTo(typeof(HomePage), user);
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login: " + ex.Message;
                System.Windows.MessageBox.Show($"An error occurred during login: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}