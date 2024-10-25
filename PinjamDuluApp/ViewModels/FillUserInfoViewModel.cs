using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using PinjamDuluApp.Views;

namespace PinjamDuluApp.ViewModels
{
    public class FillUserInfoViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        private string _fullName;
        private string _username;
        private DateTime? _birthDate;
        private string _address;
        private string _city;
        private string _contact;
        private byte[] _profilePicture;
        private string _errorMessage;
        private string _email; // From previous SignUpPage
        private string _password; // From previous SignUpPage

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string Contact
        {
            get => _contact;
            set => SetProperty(ref _contact, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand CreateAccountCommand { get; }
        public ICommand UploadProfilePictureCommand { get; }

        public FillUserInfoViewModel(NavigationService navigationService, string email, string password)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;
            _email = email;
            _password = password;

            CreateAccountCommand = new RelayCommand(async () => await CreateAccount(), CanCreateAccount);
            UploadProfilePictureCommand = new RelayCommand(UploadProfilePicture);
        }

        private bool CanCreateAccount()
        {
            return !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Address) &&
                   !string.IsNullOrWhiteSpace(City) &&
                   !string.IsNullOrWhiteSpace(Contact) &&
                   BirthDate.HasValue &&
                   _profilePicture != null;
        }

        private void UploadProfilePicture()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _profilePicture = File.ReadAllBytes(openFileDialog.FileName);
                    // Trigger CanExecute of CreateAccountCommand
                    (CreateAccountCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error uploading profile picture: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    ErrorMessage = "*Error uploading profile picture: " + ex.Message;
                }
            }
        }

        private async Task CreateAccount()
        {
            try
            {
                var user = new User
                {
                    FullName = FullName,
                    Username = Username,
                    Email = _email,
                    BirthDate = BirthDate,
                    Address = Address,
                    City = City,
                    Contact = Contact,
                    ProfilePicture = _profilePicture
                };

                if (await _databaseService.CreateUser(user, _password))
                {
                    _navigationService.NavigateTo(typeof(HomePage), user);
                }
                else
                {
                    ErrorMessage = "*Failed to create account. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "*An error occurred while creating your account: " + ex.Message;
                //System.Windows.MessageBox.Show($"Error creating account: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK);
            }
        }
    }
}