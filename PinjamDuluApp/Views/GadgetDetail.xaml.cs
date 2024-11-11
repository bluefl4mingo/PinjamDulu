using PinjamDuluApp.Models;
using PinjamDuluApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinjamDuluApp.Views
{
    /// <summary>
    /// Interaction logic for GadgetDetail.xaml
    /// </summary>
    public partial class GadgetDetail : Page
    {
        public GadgetDetail(User user, Gadget gadget)
        {
            DataContext = new GadgetDetailViewModel(MainWindow.NavigationService, user, gadget);
            InitializeComponent();
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
    }
}
