using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace PinjamDuluApp.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuration;

        public static IConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                // Read embedded appsettings.json
                string json = LoadEmbeddedAppSettings();

                // Create a temporary JSON file in memory
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Position = 0;

                    var builder = new ConfigurationBuilder()
                        .AddJsonStream(stream);

                    _configuration = builder.Build();
                }
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

        private static string LoadEmbeddedAppSettings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("PinjamDuluApp.appsettings.json"))
            {
                if (stream == null)
                    throw new FileNotFoundException("Embedded appsettings.json not found.");

                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();

                    // Add decryption logic here if needed
                    return json;
                }
            }
        }
    }
}
