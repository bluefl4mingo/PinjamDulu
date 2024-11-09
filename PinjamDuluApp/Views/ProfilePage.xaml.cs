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
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage(User user)
        {
            DataContext = new ProfileViewModel(MainWindow.NavigationService, user);
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits and the decimal point
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != ".";
        }
    }
}

