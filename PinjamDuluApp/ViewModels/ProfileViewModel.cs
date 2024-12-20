﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using PinjamDuluApp.Views;

namespace PinjamDuluApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;
        private User _currentUser;
        //private bool _isEditDialogOpen;
        private Visibility _isPopupOverlayVisible = Visibility.Collapsed;
        private Visibility _isOverlayVisible = Visibility.Collapsed;

        // Properties for the edit dialog
        private string _editFullName;
        private string _editUsername;
        private DateTime? _editBirthDate;
        private string _editAddress;
        private string _editCity;
        private string _editContact;
        private byte[] _editProfilePicture;
        private string _searchQuery;

        public ICommand EditProfileCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand NavigateToRentalCommand { get; }
        public ICommand SearchCommand { get; }

        public ProfileViewModel(NavigationService navigationService, User user)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;
            _currentUser = user;

            EditProfileCommand = new RelayCommand(OpenEditDialog);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            CancelEditCommand = new RelayCommand(CancelEdit);
            UploadImageCommand = new RelayCommand(UploadImage);
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
            SignOutCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(LoginPage)));
            NavigateToHomeCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(HomePage), user));
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            NavigateToRentalCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(RentalPage), user));
            SearchCommand = new RelayCommand(ExecuteSearch);

            LoadUserProfile(user);
        }

        private async void LoadUserProfile(User user)
        {
            // Replace this with actual logged-in user ID
            var userId = user.UserId; // Assuming you store the current user ID in App
            CurrentUser = await _databaseService.GetUserProfile(userId);
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

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        //public bool IsEditDialogOpen
        //{
        //    get => _isEditDialogOpen;
        //    set
        //    {
        //        _isEditDialogOpen = value;
        //        OnPropertyChanged(nameof(IsEditDialogOpen));
        //    }
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
            set
            {
                _isPopupOverlayVisible = value;
                OnPropertyChanged(nameof(IsPopupOverlayVisible));
            }
        }

        // Edit properties
        public string EditFullName
        {
            get => _editFullName;
            set
            {
                _editFullName = value;
                OnPropertyChanged(nameof(EditFullName));
            }
        }

        public string EditUsername
        {
            get => _editUsername;
            set
            {
                _editUsername = value;
                OnPropertyChanged(nameof(EditUsername));
            }
        }

        public DateTime? EditBirthDate
        {
            get => _editBirthDate;
            set
            {
                _editBirthDate = value;
                OnPropertyChanged(nameof(EditBirthDate));
            }
        }

        public string EditAddress
        {
            get => _editAddress;
            set
            {
                _editAddress = value;
                OnPropertyChanged(nameof(EditAddress));
            }
        }

        public string EditCity
        {
            get => _editCity;
            set
            {
                _editCity = value;
                OnPropertyChanged(nameof(EditCity));
            }
        }

        public string EditContact
        {
            get => _editContact;
            set
            {
                _editContact = value;
                OnPropertyChanged(nameof(EditContact));
            }
        }


        private void OpenEditDialog()
        {
            EditFullName = CurrentUser.FullName;
            EditUsername = CurrentUser.Username;
            EditBirthDate = CurrentUser.BirthDate;
            EditAddress = CurrentUser.Address;
            EditCity = CurrentUser.City;
            EditContact = CurrentUser.Contact;
            _editProfilePicture = CurrentUser.ProfilePicture;
            //IsEditDialogOpen = true;
            IsOverlayVisible = Visibility.Visible;
            IsPopupOverlayVisible = Visibility.Visible;
        }

        private async void SaveChanges()
        {
            try
            {
                var updatedUser = new User
                {
                    UserId = CurrentUser.UserId,
                    FullName = EditFullName,
                    Username = EditUsername,
                    Email = CurrentUser.Email, // Email remains unchanged
                    BirthDate = EditBirthDate,
                    Address = EditAddress,
                    City = EditCity,
                    Contact = EditContact,
                    ProfilePicture = _editProfilePicture ?? CurrentUser.ProfilePicture
                };

                await _databaseService.UpdateUserProfile(updatedUser);
                CurrentUser = updatedUser;
                //IsEditDialogOpen = false;
                IsPopupOverlayVisible = Visibility.Collapsed;
                IsOverlayVisible = Visibility.Collapsed;
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            //IsEditDialogOpen = false;
            IsPopupOverlayVisible = Visibility.Collapsed;
            IsOverlayVisible = Visibility.Collapsed;
        }

        private void UploadImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _editProfilePicture = File.ReadAllBytes(openFileDialog.FileName);
                    OnPropertyChanged(nameof(CurrentUser)); // Trigger UI update
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchParams = new SearchParameters { Query = SearchQuery, User = CurrentUser };

                // UNCOMMENT KALO MAU BUAT SEARCH PAGE
                _navigationService.NavigateTo(typeof(SearchPage), searchParams);
            }
        }
    }
}
