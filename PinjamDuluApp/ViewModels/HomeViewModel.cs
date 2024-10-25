using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using PinjamDuluApp.Views;

namespace PinjamDuluApp.ViewModels
{
    public class NavigationParameters
    {
        public User User { get; set; }
        public Gadget Gadget { get; set; }

        public NavigationParameters(User user, Gadget gadget)
        {
            User = user;
            Gadget = gadget;
        }
    }
    public class HomeViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<Gadget> _gadgets;
        private bool _isLoading;

        public ObservableCollection<Gadget> Gadgets
        {
            get => _gadgets;
            set
            {
                _gadgets = value;
                OnPropertyChanged(nameof(Gadgets));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand SignOutCommand { get; }
        public ICommand GadgetSelectedCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand NavigateToRentalCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToSearchCommand { get; }

        public HomeViewModel(NavigationService navigationService, User user)
        {
            _navigationService = navigationService;
            _currentUser = user;
            _databaseService = new DatabaseService();
            Gadgets = new ObservableCollection<Gadget>();

            // Initialize commands
            GadgetSelectedCommand = new RelayCommand<Gadget>(OnGadgetSelected);

            SignOutCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(LoginPage)));

            //UNCOMMENT LINE DI BAWAH KALO READY BUAT PAGE: ListingPage.xaml, RentalPage.xaml, ProfilePage.xaml, SearchPage.xaml
            //NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            //NavigateToRentalCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(RentalPage), user));
            //NavigateToProfileCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ProfilePage), user));
            //NavigateToSearchCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(SearchPage), user));

            // Load gadgets when view model is created
            LoadGadgetsAsync();
        }

        private async void LoadGadgetsAsync()
        {
            try
            {
                IsLoading = true;
                var gadgets = await _databaseService.GetRandomGadgets();
                Gadgets.Clear();
                foreach (var gadget in gadgets)
                {
                    Gadgets.Add(gadget);
                }
            }
            catch (Exception ex)
            {
                 System.Windows.MessageBox.Show($"Error loading gadgets: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnGadgetSelected(Gadget gadget)
        {
            if (gadget != null)
            {
                var navigationParams = new NavigationParameters(_currentUser, gadget);

                // UNCOMMENT LINE DI BAWAH KALO UDAH READY NGODING PAGE: GadgetDetail.xaml
                //_navigationService.NavigateTo(typeof(GadgetDetail), navigationParams.User, navigationParams.Gadget);
            }
        }
    }
}