using PinjamDuluApp.Models;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PinjamDuluApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            User user = new User("Adzka");
            base.OnStartup(e);
        }
    }

}
