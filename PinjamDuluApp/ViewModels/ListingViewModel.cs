using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using NavigationService = PinjamDuluApp.Services.NavigationService;
using PinjamDuluApp.Views;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using System.Windows;

namespace PinjamDuluApp.ViewModels
{
    public class ListingViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;
        private readonly User _currentUser;
        private ObservableCollection<Gadget> _allGadgets;
        private ObservableCollection<Gadget> _rentedGadgets;
        private bool _isAllGadgetsVisible = true;
        //private bool _isAddEditWindowVisible;
        private Visibility _isPopupOverlayVisible = Visibility.Collapsed;
        private Visibility _isOverlayVisible = Visibility.Collapsed;
        private Gadget _selectedGadget;
        private bool _isEditing;
        private string _windowTitle;
        private byte[][] _selectedImages;
        private string _searchQuery;

        private bool _isLoading;
        private string _errorMessage;

        public ListingViewModel(NavigationService navigationService, User user)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;
            _currentUser = user;

            // Initialize gadget lists to prevent null issues
            AllGadgets = new ObservableCollection<Gadget>();
            RentedGadgets = new ObservableCollection<Gadget>();

            // Load gadgets and ensure visibility state is set
            ShowAllGadgets();
            LoadGadgets();


            ShowAllGadgetsCommand = new RelayCommand(ShowAllGadgets);
            ShowRentedGadgetsCommand = new RelayCommand(ShowRentedGadgets);
            ShowAddWindowCommand = new RelayCommand(ShowAddWindow);
            EditGadgetCommand = new RelayCommand<Gadget>(EditGadget);
            SaveGadgetCommand = new RelayCommand(SaveGadget);
            SelectImagesCommand = new RelayCommand(SelectImages);
            CancelCommand = new RelayCommand(CloseAddEditWindow);
            NavigateToHomeCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(HomePage), user));
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            NavigateToRentalCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(RentalPage), user));
            NavigateToProfileCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ProfilePage), user));
            SearchCommand = new RelayCommand(ExecuteSearch);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }

        public ObservableCollection<string> CategoryOptions { get; } = new ObservableCollection<string> {"Handphone", "Tablet", "Laptop", "Smartwatch", "Headphone", "Drone"};

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));

            }
        }

        public ObservableCollection<Gadget> AllGadgets
        {
            get => _allGadgets;
            set
            {
                SetProperty(ref _allGadgets, value);
                OnPropertyChanged(nameof(AllGadgets));
            }
        }

        public ObservableCollection<Gadget> RentedGadgets
        {
            get => _rentedGadgets;
            set
            {
                SetProperty(ref _rentedGadgets, value);
                OnPropertyChanged(nameof(RentedGadgets));
            }
        }

        public bool IsAllGadgetsVisible
        {
            get => _isAllGadgetsVisible;
            set => SetProperty(ref _isAllGadgetsVisible, value);
        }

        //public bool IsAddEditWindowVisible
        //{
        //    get => _isAddEditWindowVisible;
        //    set => SetProperty(ref _isAddEditWindowVisible, value);
        //}

        public Visibility IsOverlayVisible
        {
            get => _isOverlayVisible;
            set
            {
                _isOverlayVisible = value;
                OnPropertyChanged(nameof(IsOverlayVisible));
            }
        }

        public Visibility IsPopupOverlayVisible
        {
            get => _isPopupOverlayVisible;
            set => SetProperty(ref _isPopupOverlayVisible, value);
        }

        public Gadget SelectedGadget
        {
            get => _selectedGadget;
            set => SetProperty(ref _selectedGadget, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }


        public ICommand ShowAllGadgetsCommand { get; }
        public ICommand ShowRentedGadgetsCommand { get; }
        public ICommand ShowAddWindowCommand { get; }
        public ICommand EditGadgetCommand { get; }
        public ICommand SaveGadgetCommand { get; }
        public ICommand SelectImagesCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand NavigateToRentalCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand SearchCommand { get; }

        private async void LoadGadgets()
        {
            try
            {
                IsLoading = true;
                var gadgets = await _databaseService.GetUserGadgets(_currentUser.UserId);
                AllGadgets.Clear();
                foreach (var gadget in gadgets)
                {
                    AllGadgets.Add(gadget);

                    //Gadget is rented if there is a renter awokaokw
                    if (gadget.CurrentRenterUsername != null)
                    {
                        RentedGadgets.Add(gadget);
                    }
                }

            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading gadgets: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private void ShowAllGadgets()
        {
            IsAllGadgetsVisible = true;
            OnPropertyChanged(nameof(AllGadgets)); // Force UI to refresh
        }

        private void ShowRentedGadgets()
        {
            IsAllGadgetsVisible = false;
            OnPropertyChanged(nameof(RentedGadgets)); // Force UI to refresh
        }


        private void ShowAddWindow()
        {
            _isEditing = false;
            WindowTitle = "Tambah Gadget";
            SelectedGadget = new Gadget
            {
                OwnerId = _currentUser.UserId,
                Availability = true
            };
            _selectedImages = null;
            //IsAddEditWindowVisible = true;
            IsOverlayVisible = Visibility.Visible;
            IsPopupOverlayVisible = Visibility.Visible;
        }

        private void EditGadget(Gadget gadget)
        {
            _isEditing = true;
            WindowTitle = "Edit Gadget";
            SelectedGadget = new Gadget
            {
                GadgetId = gadget.GadgetId,
                OwnerId = gadget.OwnerId,
                Title = gadget.Title,
                Description = gadget.Description,
                Category = gadget.Category,
                Brand = gadget.Brand,
                ConditionMetric = gadget.ConditionMetric,
                RentalPrice = gadget.RentalPrice,
                AvailabilityDate = gadget.AvailabilityDate,
                Images = gadget.Images
            };
            _selectedImages = gadget.Images;
            //IsAddEditWindowVisible = true;
            IsOverlayVisible = Visibility.Visible;
            IsPopupOverlayVisible = Visibility.Visible;
        }

        private async void SaveGadget()
        {
            if (SelectedGadget == null) return;

            // Validate condition metric
            if (SelectedGadget.ConditionMetric < 1 || SelectedGadget.ConditionMetric > 10)
            {
                ErrorMessage = "Kondisi gadget harus berupa bilangan bulat antara 1 hingga 10!";
                return;
            }

            // Validate rental price
            if (SelectedGadget.RentalPrice < 0)
            {
                ErrorMessage = "Harga rental tidak bisa negatif!";
                return;
            }

            if (_selectedImages != null)
            {
                SelectedGadget.Images = _selectedImages;
            }

            try
            {
                bool success;
                if (_isEditing)
                {
                    (success, ErrorMessage) = await _databaseService.UpdateGadget(SelectedGadget);
                }
                else
                {
                    (success, ErrorMessage) = await _databaseService.AddGadget(SelectedGadget);
                }

                if (success)
                {
                    LoadGadgets();
                    CloseAddEditWindow();
                }
                else
                {
                    ErrorMessage = "Failed to save the gadget: " + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while saving the gadget: " + ex.Message;
            }
        }


        private void SelectImages()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imageFiles = openFileDialog.FileNames;
                var imageList = new List<byte[]>();

                foreach (var file in imageFiles.Take(4)) // Limit to 4 images
                {
                    imageList.Add(File.ReadAllBytes(file));
                }

                _selectedImages = imageList.ToArray();
            }
        }

        private void CloseAddEditWindow()
        {
            //IsAddEditWindowVisible = false;
            IsPopupOverlayVisible = Visibility.Collapsed;
            IsOverlayVisible = Visibility.Collapsed;
            SelectedGadget = null;
            _selectedImages = null;
        }

        private void ExecuteSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchParams = new SearchParameters { Query = SearchQuery, User = _currentUser };

                // UNCOMMENT KALO MAU BUAT SEARCH PAGE
                _navigationService.NavigateTo(typeof(SearchPage), searchParams);
            }
        }
    }
}