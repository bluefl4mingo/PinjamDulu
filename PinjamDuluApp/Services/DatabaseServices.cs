using Npgsql;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using System.Text;
using System.Security.Cryptography;

namespace PinjamDuluApp.Services
{

    public class DatabaseService
    {
        //--------------------------- AUTHENTICATION SERVICES (services for Page: Login, SignUp, FIll User Information) ---------------------------//

        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationHelper.GetConnectionString();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<User> AuthenticateUser(string email, string password)
        {
            string hashedPassword = HashPassword(password);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"SELECT user_id, full_name, username, email, birth_date, address, city, contact, profile_picture 
                           FROM public.""User"" 
                           WHERE email = @email AND password = @password";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("password", hashedPassword);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetGuid(0),
                                FullName = reader.GetString(1),
                                Username = reader.GetString(2),
                                Email = reader.GetString(3),
                                BirthDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                                City = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Contact = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ProfilePicture = reader.IsDBNull(8) ? null : (byte[])reader[8]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"SELECT COUNT(*) FROM public.""User"" WHERE email = @email";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<bool> CreateUser(User user, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        INSERT INTO public.""User"" (user_id, full_name, username, email, password, birth_date, address, city, contact, profile_picture)
                        VALUES (@userId, @fullName, @username, @email, @password, @birthDate, @address, @city, @contact, @profilePicture)";

                    cmd.Parameters.AddWithValue("userId", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("fullName", user.FullName);
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("email", user.Email);
                    cmd.Parameters.AddWithValue("password", HashPassword(password));
                    cmd.Parameters.AddWithValue("birthDate", (object)user.BirthDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("address", (object)user.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("city", (object)user.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("contact", (object)user.Contact ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("profilePicture", user.ProfilePicture);

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to create user: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return false;
                    }
                }
            }
        }



        //--------------------------- HOME PAGE SERVICES (services for Page: Home Page) ---------------------------//
        public async Task<List<Gadget>> GetRandomGadgets(int count = 20)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var gadgets = new List<Gadget>();

                var sql = @"
                    SELECT g.*, 
                           gi.image[1] as first_image,
                           COUNT(DISTINCT b.booking_id) as times_rented,
                           u.city as owner_city
                    FROM public.""Gadget"" g
                    LEFT JOIN public.""GadgetImages"" gi ON g.gadget_id = gi.gadget_id
                    LEFT JOIN public.""Booking"" b ON g.gadget_id = b.gadget_id
                    LEFT JOIN public.""User"" u ON g.owner_id = u.user_id
                    GROUP BY g.gadget_id, gi.image[1], u.city
                    ORDER BY RANDOM()
                    LIMIT @count";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("count", count);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var gadget = new Gadget
                            {
                                GadgetId = reader.GetGuid(reader.GetOrdinal("gadget_id")),
                                OwnerId = reader.GetGuid(reader.GetOrdinal("owner_id")),
                                Title = reader.GetString(reader.GetOrdinal("title")),
                                Description = reader.GetString(reader.GetOrdinal("description")),
                                Category = reader.GetString(reader.GetOrdinal("category")),
                                Brand = reader.GetString(reader.GetOrdinal("brand")),
                                ConditionMetric = reader.GetInt32(reader.GetOrdinal("condition_metric")),
                                GadgetRating = reader.GetFloat(reader.GetOrdinal("gadget_rating")),
                                RentalPrice = reader.GetDecimal(reader.GetOrdinal("rental_price")),
                                Availability = reader.GetBoolean(reader.GetOrdinal("availability")),
                                AvailabilityDate = reader.IsDBNull(reader.GetOrdinal("availability_date")) ? null : reader.GetDateTime(reader.GetOrdinal("availability_date")),
                                Images = reader.IsDBNull(reader.GetOrdinal("first_image")) ? null : new[] { (byte[])reader["first_image"] },
                                TimesRented = reader.GetInt32(reader.GetOrdinal("times_rented")),
                                OwnerCity = reader.IsDBNull(reader.GetOrdinal("owner_city")) ? "Unknown" : reader.GetString(reader.GetOrdinal("owner_city"))
                            };
                            gadgets.Add(gadget);
                        }
                    }
                }
                return gadgets;
            }
        }


        //--------------------------- PROFILE SERVICES ---------------------------//
        public async Task<User> GetUserProfile(Guid userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"SELECT * FROM public.""User"" WHERE user_id = @userId";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                                FullName = reader.GetString(reader.GetOrdinal("full_name")),
                                Username = reader.GetString(reader.GetOrdinal("username")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                BirthDate = reader.IsDBNull(reader.GetOrdinal("birth_date")) ? null : reader.GetDateTime(reader.GetOrdinal("birth_date")),
                                Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                                City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city")),
                                Contact = reader.IsDBNull(reader.GetOrdinal("contact")) ? null : reader.GetString(reader.GetOrdinal("contact")),
                                ProfilePicture = reader.IsDBNull(reader.GetOrdinal("profile_picture")) ? null : (byte[])reader["profile_picture"]
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task UpdateUserProfile(User user)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"
            UPDATE public.""User""
            SET full_name = @fullName,
                username = @username,
                birth_date = @birthDate,
                address = @address,
                city = @city,
                contact = @contact,
                profile_picture = @profilePicture
            WHERE user_id = @userId";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", user.UserId);
                    cmd.Parameters.AddWithValue("fullName", user.FullName);
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("birthDate", (object)user.BirthDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("address", (object)user.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("city", (object)user.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("contact", (object)user.Contact ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("profilePicture", (object)user.ProfilePicture ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}