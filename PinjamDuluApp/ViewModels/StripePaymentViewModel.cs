using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace PinjamDuluApp.ViewModels
{
    public class StripePaymentViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NavigationService _navigationService;

        private User _currentUser;
        private Guid _gadgetId;
        private DateTime _rentEndDate;
        private decimal _totalPrice;

        public decimal TotalPrice => _totalPrice;

        public ICommand PayCommand { get; }
        public ICommand BackCommand { get; }

        public StripePaymentViewModel(NavigationService navigationService, User user, PaymentParameters rentData)
        {
            _databaseService = new DatabaseService();
            _navigationService = navigationService;

            PayCommand = new RelayCommand(async () => await ProcessPaymentAsync());
            BackCommand = new RelayCommand(() => _navigationService.GoBack());

            _currentUser = user;
            _gadgetId = rentData.GadgetId;
            _rentEndDate = rentData.RentEndDate;
            _totalPrice = rentData.TotalPrice;

            // Configure Stripe with secret key
            StripeConfiguration.ApiKey = ConfigurationHelper.GetStripeSecretKey();
        }

        private string _cardNumber;
        private string _cardExpiry;
        private string _cardCVC;
        private string _cardholderName;
        private bool _isValidCardInfo;

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                _cardNumber = value;
                OnPropertyChanged(nameof(CardNumber));
                ValidateCardInfo();
            }
        }

        public string CardExpiry
        {
            get => _cardExpiry;
            set
            {
                _cardExpiry = value;
                OnPropertyChanged(nameof(CardExpiry));
                ValidateCardInfo();
            }
        }

        public string CardCVC
        {
            get => _cardCVC;
            set
            {
                _cardCVC = value;
                OnPropertyChanged(nameof(CardCVC));
                ValidateCardInfo();
            }
        }

        public string CardholderName
        {
            get => _cardholderName;
            set
            {
                _cardholderName = value;
                OnPropertyChanged(nameof(CardholderName));
                ValidateCardInfo();
            }
        }

        public bool IsValidCardInfo
        {
            get => _isValidCardInfo;
            set
            {
                _isValidCardInfo = value;
                OnPropertyChanged(nameof(IsValidCardInfo));
            }
        }

        private void ValidateCardInfo()
        {
            IsValidCardInfo =
                !string.IsNullOrWhiteSpace(CardNumber) && CardNumber.Replace(" ", "").Length == 16 &&
                !string.IsNullOrWhiteSpace(CardExpiry) && CardExpiry.Length == 5 &&
                !string.IsNullOrWhiteSpace(CardCVC) && CardCVC.Length == 3 &&
                !string.IsNullOrWhiteSpace(CardholderName);
        }


        private string GetSelectedPaymentMethod()
        {
            var selectedIndex = System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var window = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
                if (window != null)
                {
                    var comboBox = window.FindName("TestScenarioComboBox") as ComboBox;
                    return comboBox?.SelectedIndex ?? 0;
                }
                return 0;
            });

            switch (selectedIndex)
            {
                case 1:
                    return "pm_card_visa_chargeDeclined";
                case 2:
                    return "pm_card_visa_authenticationRequired";
                default:
                    return "pm_card_visa";
            }
        }

        private async Task ProcessPaymentAsync()
        {
            try
            {
                var amountInIDR = (long)Math.Round(_totalPrice * 10);

                // For mock purposes, we'll consider 4242 4242 4242 4242 as success
                var cleanCardNumber = CardNumber.Replace(" ", "");
                bool isSuccessCard = cleanCardNumber == "4242424242424242";

                if (!isSuccessCard)
                {
                    MessageBox.Show("Payment failed: Invalid test card number. Please use 4242 4242 4242 4242 for testing.",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = amountInIDR,  // No need to multiply by 100 for IDR
                    Currency = "idr",      // Set currency to IDR
                    PaymentMethod = GetSelectedPaymentMethod(),
                    Confirm = true,
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                if (paymentIntent.Status == "succeeded")
                {
                    // Create booking
                    var booking = new Booking
                    {
                        BookingId = Guid.NewGuid(),
                        GadgetId = _gadgetId,
                        BorrowerId = _currentUser.UserId,
                        LenderId = await _databaseService.GetGadgetOwnerId(_gadgetId),
                        BookingDate = DateTime.Now,
                        RentalStartDate = DateTime.Now,
                        RentalEndDate = _rentEndDate
                    };

                    await _databaseService.CreateBooking(booking);

                    // Create payment record
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        BookingId = booking.BookingId,
                        Amount = _totalPrice,
                        PaymentMethod = "Visa",
                        PaymentStatus = true,
                        TransactionDate = DateTime.Now
                    };

                    await _databaseService.CreatePayment(payment);

                    MessageBox.Show($"Payment successful, gadget {await _databaseService.GetGadgetTitle(_gadgetId)} is rented successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    _navigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Payment failed. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (StripeException e)
            {
                MessageBox.Show($"Payment failed: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
