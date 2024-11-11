using PinjamDuluApp.Models;
using PinjamDuluApp.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace PinjamDuluApp.Views
{
    /// <summary>
    /// Interaction logic for StripePayment.xaml
    /// </summary>
    public partial class StripePayment : Page
    {
        public StripePayment(User user, PaymentParameters rentData)
        {
            DataContext = new StripePaymentViewModel(MainWindow.NavigationService, user, rentData);
            InitializeComponent();
        }

        private void NumberOnlyPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void CardNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(" ", "");
            if (text.Length > 0)
            {
                // Format card number in groups of 4
                text = string.Join(" ", Regex.Matches(text, ".{1,4}").Cast<Match>().Select(m => m.Value));
                if (text != textBox.Text)
                {
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                }
            }
        }

        private void ExpiryTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace("/", "");
            if (text.Length > 0)
            {
                // Format expiry as MM/YY
                if (text.Length >= 2)
                {
                    text = text.Insert(2, "/");
                }
                if (text != textBox.Text)
                {
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                }
            }
        }
    }
}
