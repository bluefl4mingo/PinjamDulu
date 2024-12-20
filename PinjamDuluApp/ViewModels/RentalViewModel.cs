﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.Views;
using PinjamDuluApp.ViewModels;

namespace PinjamDuluApp.ViewModels
{
    public class RentalViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        public ObservableCollection<RentalItem> AllRentals { get; set; }
        public ObservableCollection<RentalItem> DisplayedRentals { get; set; }
        private RentalItem _selectedRental;
        private readonly User _currentUser;
        private int _selectedRating;
        private Visibility _isPopupOverlayVisible = Visibility.Collapsed;
        private Visibility _isOverlayVisible = Visibility.Collapsed;
        private string _reviewText;
        private string _searchQuery;
        //private bool _isReviewPopupOpen;
        private bool _isCurrentRentalsVisible = true;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }

        public bool IsCurrentRentalsVisible
        {
            get => _isCurrentRentalsVisible;
            set
            {
                _isCurrentRentalsVisible = value;
                OnPropertyChanged(nameof(IsCurrentRentalsVisible));
                UpdateDisplayedRentals();
            }
        }

        //public bool IsReviewPopupOpen
        //{
        //    get => _isReviewPopupOpen;
        //    set
        //    {
        //        _isReviewPopupOpen = value;
        //        OnPropertyChanged(nameof(IsReviewPopupOpen));
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
            set => SetProperty(ref _isPopupOverlayVisible, value);
        }

        public RentalItem SelectedRental
        {
            get => _selectedRental;
            set
            {
                _selectedRental = value;
                OnPropertyChanged(nameof(SelectedRental));
            }
        }

        public ObservableCollection<int> RatingOptions { get; } = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

        
        public int SelectedRating
        {
            get => _selectedRating;
            set
            {
                _selectedRating = value;
                OnPropertyChanged(nameof(SelectedRating));
            }
        }
        
        public string ReviewText
        {
            get => _reviewText;
            set
            {
                _reviewText = value;
                OnPropertyChanged(nameof(ReviewText));
            }
        }

        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand NavigateToRentalCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand ShowCurrentRentalsCommand { get; }
        public ICommand ShowRentalHistoryCommand { get; }
        public ICommand CompleteRentCommand { get; }
        public ICommand SubmitReviewCommand { get; }
        public ICommand CancelReviewCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GadgetSelectedCommand { get; }

        public RentalViewModel(NavigationService navigationService, User user)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;
            _currentUser = user;

            AllRentals = new ObservableCollection<RentalItem>();
            DisplayedRentals = new ObservableCollection<RentalItem>();

            NavigateToHomeCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(HomePage), user));
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            NavigateToRentalCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(RentalPage), user));
            NavigateToProfileCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ProfilePage), user));
            ShowCurrentRentalsCommand = new RelayCommand(() => IsCurrentRentalsVisible = true);
            ShowRentalHistoryCommand = new RelayCommand(() => IsCurrentRentalsVisible = false);
            CompleteRentCommand = new RelayCommand<RentalItem>(CompleteRent);
            SubmitReviewCommand = new RelayCommand(SubmitReview);
            CancelReviewCommand = new RelayCommand(CancelReview);
            SearchCommand = new RelayCommand(ExecuteSearch);
            GadgetSelectedCommand = new RelayCommand<RentalItem>(OnGadgetSelected);

            LoadRentals(user);
        }

        private void CancelReview()
        {           
            IsPopupOverlayVisible = Visibility.Collapsed;
            IsOverlayVisible = Visibility.Collapsed;
        }

        private async void LoadRentals(User user)
        {
            var rentals = await _databaseService.GetUserRentals(user.UserId);

            AllRentals.Clear();
            foreach (var rental in rentals)
            {
                AllRentals.Add(rental);
            }

            UpdateDisplayedRentals();
        }

        private void UpdateDisplayedRentals()
        {
            DisplayedRentals.Clear();
            var filteredRentals = IsCurrentRentalsVisible
                ? AllRentals.Where(r => r.RentalEndDate >= DateTime.Now && r.Review == null)
                : AllRentals.Where(r => r.RentalEndDate < DateTime.Now || r.Review != null);

            foreach (var rental in filteredRentals)
            {
                DisplayedRentals.Add(rental);
            }
        }

        private void CompleteRent(RentalItem rental)
        {
            SelectedRental = rental;
            //IsReviewPopupOpen = true;
            IsOverlayVisible = Visibility.Visible;
            IsPopupOverlayVisible = Visibility.Visible;
        }

        private async void SubmitReview()
        {
            if (SelectedRental != null)
            {
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    BookingId = SelectedRental.BookingId,
                    Rating = (short)SelectedRating,
                    ReviewText = ReviewText,
                    ReviewDate = DateTime.Now
                };

                await _databaseService.AddReview(review);
                SelectedRental.Review = review;

                //IsReviewPopupOpen = false;
                IsPopupOverlayVisible = Visibility.Collapsed;
                IsOverlayVisible = Visibility.Collapsed;
                UpdateDisplayedRentals();
            }
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

        private void OnGadgetSelected(RentalItem gadget)
        {
            if (gadget != null)
            {
                var navigationParams = new NavigationParameters(_currentUser, gadget.Gadget);
                _navigationService.NavigateTo(typeof(GadgetDetail), navigationParams.User, navigationParams.Gadget);
            }
        }
    }
}