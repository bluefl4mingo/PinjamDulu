using PinjamDuluApp.Models;
using PinjamDuluApp.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PinjamDuluApp.Views
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public SearchPage(SearchParameters searchparams)
        {
            DataContext = new SearchPageViewModel(MainWindow.NavigationService, searchparams);
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RatingValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and one decimal point
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            string newText = ((TextBox)sender).Text + e.Text;

            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }

            // Check if the value is within 0-5 range
            if (float.TryParse(newText, out float value))
            {
                e.Handled = value < 0 || value > 5;
            }
        }

        private void ConditionValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // Check if the value is within 1-10 range
            string newText = ((TextBox)sender).Text + e.Text;
            if (int.TryParse(newText, out int value))
            {
                e.Handled = value < 1 || value > 10;
            }
        }

        private void OnCategoryChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                var viewModel = (SearchPageViewModel)DataContext;
                viewModel.SelectedCategory = radioButton.Tag?.ToString();
            }
        }

        private void boxHargaMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxHargaMin.Text) && boxHargaMin.Text.Length > 0)
            {
                txtHargaMin.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtHargaMin.Visibility = Visibility.Visible;
            }
        }

        private void boxHargaMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxHargaMax.Text) && boxHargaMax.Text.Length > 0)
            {
                txtHargaMax.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtHargaMax.Visibility = Visibility.Visible;
            }
        }

        private void boxRating_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxRating.Text) && boxRating.Text.Length > 0)
            {
                txtRating.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtRating.Visibility = Visibility.Visible;
            }
        }

        private void boxCondition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxCondition.Text) && boxCondition.Text.Length > 0)
            {
                txtCondition.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtCondition.Visibility = Visibility.Visible;
            }
        }

        private void boxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxSearch.Text) && boxSearch.Text.Length > 0)
            {
                txtSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtSearch.Visibility = Visibility.Visible;
            }
        }

        private void txtSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxSearch.Focus();
        }

        private void txtHargaMin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxHargaMin.Focus();
        }

        private void txtHargaMax_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxHargaMax.Focus();
        }

        private void txtRating_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxRating.Focus();
        }

        private void txtCondition_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxCondition.Focus();
        }
    }
}
