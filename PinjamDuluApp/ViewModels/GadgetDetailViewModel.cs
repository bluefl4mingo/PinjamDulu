using PinjamDuluApp.Views;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.ViewModels;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinjamDuluApp.ViewModels
{
    internal class GadgetDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;
        private Gadget _gadget;
        public Gadget Gadget
        {
            get => _gadget;
            set
            {
                if (SetProperty(ref _gadget, value))
                {
                    // Recalculate total price when gadget changes
                    CalculateTotalPrice();
                }
            }
        }

        private DateTime _rentEndDate;
        public DateTime RentEndDate
        {
            get => _rentEndDate;
            set
            {
                if (SetProperty(ref _rentEndDate, value))
                {
                    CalculateTotalPrice();
                }
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        public DateTime TomorrowDate => DateTime.Today.AddDays(1);
        public ObservableCollection<Review> Reviews { get; private set; }
        public ICommand RentCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToListingCommand { get; }
        public ICommand NavigateToRentalCommand { get; }
        public ICommand NavigateToProfileCommand { get; }

        public GadgetDetailViewModel(NavigationService navigationService, User user, Gadget gadget)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;

            // Set the gadget immediately from the parameter
            Gadget = gadget;

            // Initialize other properties
            RentEndDate = TomorrowDate;
            Reviews = new ObservableCollection<Review>();
            RentCommand = new RelayCommand(() => InitiateRental(user));
            NavigateToHomeCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(HomePage), user));
            NavigateToListingCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ListingPage), user));
            NavigateToRentalCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(RentalPage), user));
            NavigateToProfileCommand = new RelayCommand(() => _navigationService.NavigateTo(typeof(ProfilePage), user));

            // Load additional details (reviews) asynchronously
            LoadReviewsAsync(gadget.GadgetId);
        }

        private async void LoadReviewsAsync(Guid gadgetId)
        {
            try
            {
                var reviews = await _databaseService.GetGadgetReviews(gadgetId);
                Reviews.Clear();
                foreach (var review in reviews)
                {
                    Reviews.Add(review);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reviews: {ex.Message}");
            }
        }

        private void CalculateTotalPrice()
        {
            if (Gadget != null) // Add null check for safety
            {
                int rentalDays = (RentEndDate - DateTime.Today).Days;
                TotalPrice = Gadget.RentalPrice * rentalDays;
            }
        }

        private async void InitiateRental(User user)
        {
            try
            {
                // Check if the selected rental end date is before the current date
                if (RentEndDate < DateTime.Today)
                {
                    MessageBox.Show(
                        "You cannot select a rental end date that is today's or before today's date.",
                        "Invalid Rental Date",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                var (isAvailable, message) = await _databaseService.CheckGadgetAvailabilityForRental(Gadget.GadgetId, user.UserId);

                if (!isAvailable)
                {
                    MessageBox.Show(
                        message,
                        "Rental Not Allowed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                var paymentParameters = new PaymentParameters
                {
                    GadgetId = Gadget.GadgetId,
                    RentEndDate = RentEndDate,
                    TotalPrice = TotalPrice
                };

                //UNCOMMENT KALO UDAH MAU BUAT PAYMENT PAGE
                _navigationService.NavigateTo(typeof(StripePayment), user, paymentParameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while processing your rental request. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                System.Diagnostics.Debug.WriteLine($"Rental initiation error: {ex.Message}");
            }
        }
    }

    // Add these classes to help with type safety
    public class PaymentParameters
    {
        public Guid GadgetId { get; set; }
        public DateTime RentEndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}