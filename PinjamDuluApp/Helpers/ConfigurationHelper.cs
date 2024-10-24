using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;

namespace PinjamDuluApp.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuration;

        public static IConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _configuration = builder.Build();
            }

            return _configuration;
        }

        public static string GetConnectionString()
        {
            return GetConfiguration().GetConnectionString("DefaultConnection");
        }

        public static string GetStripeSecretKey()
        {
            return GetConfiguration()["Stripe:SecretKey"];
        }

        public static string GetStripePublishableKey()
        {
            return GetConfiguration()["Stripe:PublishableKey"];
        }
    }
}
