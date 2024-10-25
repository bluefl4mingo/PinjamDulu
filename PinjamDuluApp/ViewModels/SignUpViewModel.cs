using System.Windows.Input;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using PinjamDuluApp.Views;

namespace PinjamDuluApp.ViewModels
{
    public class SignUpViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        private string _email;
        private string _password;
        private string _confirmPassword;
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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SignUpCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public SignUpViewModel(NavigationService navigationService)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;

            SignUpCommand = new RelayCommand(async () => await SignUp(), CanSignUp);
            NavigateToLoginCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(LoginPage)));
        }

        private bool CanSignUp()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Password == ConfirmPassword;
        }

        private async Task SignUp()
        {
            try
            {
                if (await _databaseService.CheckEmailExists(Email))
                {
                    ErrorMessage = "*Email already exists";
                    return;
                }

                // Navigate to FillUserInfoPage with email and password as parameters
                _navigationService.NavigateTo(typeof(FillUserInfoPage), _email, _password); // we include email and password since we need to pass those 2 data to FillUserInfoPage.
            }
            catch (Exception ex)
            {
                ErrorMessage = $"*An error occurred during sign up: {ex.Message}";
                System.Windows.MessageBox.Show($"An error occurred during sign up: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}