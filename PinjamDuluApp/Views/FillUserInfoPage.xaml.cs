using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using PinjamDuluApp.ViewModels;

namespace PinjamDuluApp.Views
{
    /// <summary>
    /// Interaction logic for FillUserInfoPage.xaml
    /// </summary>
    public partial class FillUserInfoPage : Page
    {
        private FillUserInfoViewModel _viewModel;

        public FillUserInfoPage(string email, string password)
        {
            InitializeComponent();
            _viewModel = new FillUserInfoViewModel(MainWindow.NavigationService, email, password);
            DataContext = _viewModel;
        }

        private void textFullName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxFullName.Focus();
        }

        private void textUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxUsername.Focus();
        }

        private void textBirth_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void boxFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxFullName.Text) && boxFullName.Text.Length > 0)
            {
                textFullName.Visibility = Visibility.Collapsed;
            }
            else
            {
                textFullName.Visibility = Visibility.Visible;
            }
        }

        private void boxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxUsername.Text) && boxUsername.Text.Length > 0)
            {
                textUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
                textUsername.Visibility = Visibility.Visible;
            }
        }

        private void textAddress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxAddress.Focus();
        }

        private void textCity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxCity.Focus();
        }

        private void textContact_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boxContact.Focus();
        }

        private void boxAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxAddress.Text) && boxAddress.Text.Length > 0)
            {
                textAddress.Visibility = Visibility.Collapsed;
            }
            else
            {
                textAddress.Visibility = Visibility.Visible;
            }
        }

        private void boxCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxCity.Text) && boxCity.Text.Length > 0)
            {
                textCity.Visibility = Visibility.Collapsed;
            }
            else
            {
                textCity.Visibility = Visibility.Visible;
            }
        }

        private void boxContact_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(boxContact.Text) && boxContact.Text.Length > 0)
            {
                textContact.Visibility = Visibility.Collapsed;
            }
            else
            {
                textContact.Visibility = Visibility.Visible;
            }
        }

        private void barBtn1_Click(object sender, RoutedEventArgs e)
        {
            if (infoPanel2.Visibility == Visibility.Visible)
            {
                infoPanel2.Visibility = Visibility.Collapsed;
                infoPanel1.Visibility = Visibility.Visible;
                barBtn2.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#B6B7BA");
                barBtn1.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1D1D21");
                nextBtn.Visibility = Visibility.Visible;
                createAccBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void barBtn2_Click(object sender, RoutedEventArgs e)
        {
            if (infoPanel1.Visibility == Visibility.Visible)
            {
                infoPanel1.Visibility = Visibility.Collapsed;
                infoPanel2.Visibility = Visibility.Visible;
                barBtn1.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#B6B7BA");
                barBtn2.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1D1D21");
                nextBtn.Visibility = Visibility.Collapsed;
                createAccBtn.Visibility = Visibility.Visible;
            }
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (infoPanel1.Visibility == Visibility.Visible)
            {
                infoPanel1.Visibility = Visibility.Collapsed;
                infoPanel2.Visibility = Visibility.Visible;
                barBtn1.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#B6B7BA");
                barBtn2.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1D1D21");
                nextBtn.Visibility = Visibility.Collapsed;

            }
        }
    }
}
