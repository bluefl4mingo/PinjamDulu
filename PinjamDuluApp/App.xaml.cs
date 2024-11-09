using PinjamDuluApp.Services;
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
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                // Update gadget availability status when app starts
                var databaseService = new DatabaseService();
                await databaseService.UpdateGadgetAvailabilityStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error updating gadget availability status. Some gadget statuses may be incorrect.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                System.Diagnostics.Debug.WriteLine($"Startup error: {ex.Message}");
            }
        }
    }

}
