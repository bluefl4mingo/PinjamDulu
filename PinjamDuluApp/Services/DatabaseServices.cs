using Npgsql;
using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using System.Text;
using System.Security.Cryptography;

namespace PinjamDuluApp.Services
{

    public class DatabaseService
    {
        //--------------------------- AUTHENTICATION SERVICES ---------------------------//
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



        //--------------------------- HOME PAGE SERVICES ---------------------------//
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



        //--------------------------- RENTAL SERVICES ---------------------------//
        public async Task<List<RentalItem>> GetUserRentals(Guid userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var rentals = new List<RentalItem>();
                var sql = @"
            SELECT b.booking_id, b.rental_start_date, b.rental_end_date,
                   g.gadget_id, g.title, g.description, g.category, g.brand, g.rental_price,
                   u.full_name AS owner_name,
                   r.review_id, r.rating, r.review_text, r.review_date,
                   gi.image AS gadget_images
            FROM public.""Booking"" b
            JOIN public.""Gadget"" g ON b.gadget_id = g.gadget_id
            JOIN public.""User"" u ON g.owner_id = u.user_id
            LEFT JOIN public.""Review"" r ON b.booking_id = r.booking_id
            LEFT JOIN public.""GadgetImages"" gi ON g.gadget_id = gi.gadget_id
            WHERE b.borrower_id = @userId
            ORDER BY b.rental_start_date DESC";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var rentalItem = new RentalItem
                            {
                                BookingId = reader.GetGuid(reader.GetOrdinal("booking_id")),
                                RentalStartDate = reader.GetDateTime(reader.GetOrdinal("rental_start_date")),
                                RentalEndDate = reader.GetDateTime(reader.GetOrdinal("rental_end_date")),
                                Gadget = new Gadget
                                {
                                    GadgetId = reader.GetGuid(reader.GetOrdinal("gadget_id")),
                                    Title = reader.GetString(reader.GetOrdinal("title")),
                                    Description = reader.GetString(reader.GetOrdinal("description")),
                                    Category = reader.GetString(reader.GetOrdinal("category")),
                                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                                    RentalPrice = reader.GetDecimal(reader.GetOrdinal("rental_price")),
                                    Images = reader.IsDBNull(reader.GetOrdinal("gadget_images"))
                                        ? null
                                        : (byte[][])reader["gadget_images"]
                                },
                                OwnerName = reader.GetString(reader.GetOrdinal("owner_name"))
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("review_id")))
                            {
                                rentalItem.Review = new Review
                                {
                                    ReviewId = reader.GetGuid(reader.GetOrdinal("review_id")),
                                    Rating = reader.GetInt16(reader.GetOrdinal("rating")),
                                    ReviewText = reader.GetString(reader.GetOrdinal("review_text")),
                                    ReviewDate = reader.GetDateTime(reader.GetOrdinal("review_date"))
                                };
                            }

                            rentals.Add(rentalItem);
                        }
                    }
                }
                return rentals;
            }
        }

        public async Task AddReview(Review review)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"
                            INSERT INTO public.""Review"" (review_id, booking_id, rating, review_text, review_date)
                            VALUES (@reviewId, @bookingId, @rating, @reviewText, @reviewDate)";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                    cmd.Parameters.AddWithValue("bookingId", review.BookingId);
                    cmd.Parameters.AddWithValue("rating", review.Rating);
                    cmd.Parameters.AddWithValue("reviewText", review.ReviewText);
                    cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        

        //--------------------------- LISTING GADGET SERVICES ---------------------------//
        public async Task<List<Gadget>> GetUserGadgets(Guid userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var gadgets = new List<Gadget>();

                var sql = @"
                            SELECT g.*, 
                                   gi.image[1] as first_image,
                                   COUNT(DISTINCT b.booking_id) as times_rented,
                                   current_booking.borrower_username,
                                   current_booking.rental_start_date,
                                   current_booking.rental_end_date
                            FROM public.""Gadget"" g
                            LEFT JOIN public.""GadgetImages"" gi ON g.gadget_id = gi.gadget_id
                            LEFT JOIN public.""Booking"" b ON g.gadget_id = b.gadget_id
                            LEFT JOIN (
                                SELECT b.gadget_id, 
                                       u.username as borrower_username,
                                       b.rental_start_date,
                                       b.rental_end_date
                                FROM public.""Booking"" b
                                JOIN public.""User"" u ON b.borrower_id = u.user_id
                                WHERE CURRENT_DATE BETWEEN b.rental_start_date AND b.rental_end_date
                            ) current_booking ON g.gadget_id = current_booking.gadget_id
                            WHERE g.owner_id = @userId
                            GROUP BY g.gadget_id, gi.image[1], 
                                     current_booking.borrower_username,
                                     current_booking.rental_start_date,
                                     current_booking.rental_end_date";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);

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
                                CurrentRenterUsername = reader.IsDBNull(reader.GetOrdinal("borrower_username")) ? null : reader.GetString(reader.GetOrdinal("borrower_username")),
                                CurrentRentalStart = reader.IsDBNull(reader.GetOrdinal("rental_start_date")) ? null : reader.GetDateTime(reader.GetOrdinal("rental_start_date")),
                                CurrentRentalEnd = reader.IsDBNull(reader.GetOrdinal("rental_end_date")) ? null : reader.GetDateTime(reader.GetOrdinal("rental_end_date"))
                            };
                            gadgets.Add(gadget);
                        }
                    }
                }
                return gadgets;
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddGadget(Gadget gadget)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Insert gadget
                        var gadgetSql = @"
                                        INSERT INTO public.""Gadget"" (gadget_id, owner_id, title, description, 
                                        category, brand, condition_metric, gadget_rating, rental_price, 
                                        availability, availability_date)
                                        VALUES (@gadgetId, @ownerId, @title, @description, @category, 
                                        @brand, @conditionMetric, @gadgetRating, @rentalPrice, 
                                        @availability, @availabilityDate)";

                        using (var cmd = new NpgsqlCommand(gadgetSql, conn, transaction))
                        {
                            var gadgetId = Guid.NewGuid();
                            cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                            cmd.Parameters.AddWithValue("ownerId", gadget.OwnerId);
                            cmd.Parameters.AddWithValue("title", gadget.Title);
                            cmd.Parameters.AddWithValue("description", gadget.Description);
                            cmd.Parameters.AddWithValue("category", gadget.Category);
                            cmd.Parameters.AddWithValue("brand", gadget.Brand);
                            cmd.Parameters.AddWithValue("conditionMetric", gadget.ConditionMetric);
                            cmd.Parameters.AddWithValue("gadgetRating", 0); // New gadget starts with 0 rating
                            cmd.Parameters.AddWithValue("rentalPrice", gadget.RentalPrice);
                            cmd.Parameters.AddWithValue("availability", true);
                            cmd.Parameters.AddWithValue("availabilityDate", (object)gadget.AvailabilityDate ?? DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();

                            // Insert images
                            if (gadget.Images != null && gadget.Images.Length > 0)
                            {
                                var imagesSql = @"
                                                INSERT INTO public.""GadgetImages"" (gadget_image_id, gadget_id, image)
                                                VALUES (@imageId, @gadgetId, @image)";

                                using (var imgCmd = new NpgsqlCommand(imagesSql, conn, transaction))
                                {
                                    imgCmd.Parameters.Clear();
                                    imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                    imgCmd.Parameters.AddWithValue("gadgetId", gadgetId);
                                    imgCmd.Parameters.AddWithValue("image", gadget.Images);
                                    await imgCmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return (true, string.Empty); // Operation succeeded, no error message
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return (false, ex.Message); // Return error message
                    }
                }
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateGadget(Gadget gadget)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var sql = @"
                                    UPDATE public.""Gadget""
                                    SET title = @title,
                                        description = @description,
                                        category = @category,
                                        brand = @brand,
                                        condition_metric = @conditionMetric,
                                        rental_price = @rentalPrice,
                                        availability_date = @availabilityDate
                                    WHERE gadget_id = @gadgetId";

                        using (var cmd = new NpgsqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                            cmd.Parameters.AddWithValue("title", gadget.Title);
                            cmd.Parameters.AddWithValue("description", gadget.Description);
                            cmd.Parameters.AddWithValue("category", gadget.Category);
                            cmd.Parameters.AddWithValue("brand", gadget.Brand);
                            cmd.Parameters.AddWithValue("conditionMetric", gadget.ConditionMetric);
                            cmd.Parameters.AddWithValue("rentalPrice", gadget.RentalPrice);
                            cmd.Parameters.AddWithValue("availabilityDate", (object)gadget.AvailabilityDate ?? DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Update images if new ones are provided
                        if (gadget.Images != null && gadget.Images.Length > 0)
                        {
                            // Delete existing images
                            var deleteImagesSql = @"DELETE FROM public.""GadgetImages"" WHERE gadget_id = @gadgetId";
                            using (var delCmd = new NpgsqlCommand(deleteImagesSql, conn, transaction))
                            {
                                delCmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                                await delCmd.ExecuteNonQueryAsync();
                            }

                            // Insert new images
                            var insertImagesSql = @"
                                                    INSERT INTO public.""GadgetImages"" (gadget_image_id, gadget_id, image)
                                                    VALUES (@imageId, @gadgetId, @image)";

                            using (var imgCmd = new NpgsqlCommand(insertImagesSql, conn, transaction))
                            {
                                imgCmd.Parameters.Clear();
                                imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                imgCmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                                imgCmd.Parameters.AddWithValue("image", gadget.Images);
                                await imgCmd.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                        return (true, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return (false, ex.Message); // Return error message
                    }
                }
            }
        }



        //--------------------------- GADGET DETAIL PAGE SERVICES ---------------------------//

        public async Task<List<Review>> GetGadgetReviews(Guid gadgetId)
        {
            var reviews = new List<Review>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"SELECT r.*, u.full_name as reviewer_name, u.profile_picture
                            FROM public.""Review"" r
                            JOIN public.""Booking"" b ON r.booking_id = b.booking_id
                            JOIN public.""User"" u ON b.borrower_id = u.user_id
                            WHERE b.gadget_id = @gadgetId
                            ORDER BY r.review_date DESC";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reviews.Add(new Review
                            {
                                ReviewId = reader.GetGuid(reader.GetOrdinal("review_id")),
                                BookingId = reader.GetGuid(reader.GetOrdinal("booking_id")),
                                Rating = reader.GetInt16(reader.GetOrdinal("rating")),
                                ReviewText = reader.IsDBNull(reader.GetOrdinal("review_text")) ? null : reader.GetString(reader.GetOrdinal("review_text")),
                                ReviewDate = reader.GetDateTime(reader.GetOrdinal("review_date")),
                                ReviewerName = reader.GetString(reader.GetOrdinal("reviewer_name")),
                                ReviewerProfilePicture = (byte[])reader["profile_picture"]
                            });
                        }
                    }
                }
            }
            return reviews;
        }

        public async Task<Guid> GetGadgetOwnerId(Guid gadgetId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = "SELECT owner_id FROM public.\"Gadget\" WHERE gadget_id = @gadgetId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                    return (Guid)await cmd.ExecuteScalarAsync();
                }
            }
        }

        public async Task<string> GetGadgetTitle(Guid gadgetId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = "SELECT title FROM public.\"Gadget\" WHERE gadget_id = @gadgetId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                    return (string)await cmd.ExecuteScalarAsync();
                }
            }
        }

        public async Task<(bool isAvailable, string message)> CheckGadgetAvailabilityForRental(Guid gadgetId, Guid userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // First check if user owns this gadget
                var ownershipSql = @"
                                    SELECT owner_id 
                                    FROM public.""Gadget"" 
                                    WHERE gadget_id = @gadgetId";

                using (var cmd = new NpgsqlCommand(ownershipSql, conn))
                {
                    cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                    var ownerId = await cmd.ExecuteScalarAsync() as Guid?;

                    if (ownerId == userId)
                    {
                        return (false, "You cannot rent your own gadget.");
                    }
                }

                // Then check if gadget is currently rented
                var rentalSql = @"
                                SELECT COUNT(*) 
                                FROM public.""Booking"" 
                                WHERE gadget_id = @gadgetId 
                                AND CURRENT_DATE BETWEEN rental_start_date AND rental_end_date";

                using (var cmd = new NpgsqlCommand(rentalSql, conn))
                {
                    cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                    var activeRentals = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    if (activeRentals > 0)
                    {
                        return (false, "This gadget is currently being rented by another user.");
                    }
                }

                return (true, "Available for rental");
            }
        }



        //--------------------------- PAYMENT & BOOKING SERVICES ---------------------------//
        public async Task CreateBooking(Booking booking)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Begin transaction
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Insert the booking
                        var bookingSql = @"
                    INSERT INTO public.""Booking"" (
                        booking_id, gadget_id, borrower_id, lender_id, 
                        booking_date, rental_start_date, rental_end_date
                    )
                    VALUES (
                        @bookingId, @gadgetId, @borrowerId, @lenderId, 
                        @bookingDate, @rentalStartDate, @rentalEndDate
                    )";

                        using (var cmd = new NpgsqlCommand(bookingSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("bookingId", booking.BookingId);
                            cmd.Parameters.AddWithValue("gadgetId", booking.GadgetId);
                            cmd.Parameters.AddWithValue("borrowerId", booking.BorrowerId);
                            cmd.Parameters.AddWithValue("lenderId", booking.LenderId);
                            cmd.Parameters.AddWithValue("bookingDate", booking.BookingDate);
                            cmd.Parameters.AddWithValue("rentalStartDate", booking.RentalStartDate);
                            cmd.Parameters.AddWithValue("rentalEndDate", booking.RentalEndDate);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 2. Update the gadget availability
                        var gadgetUpdateSql = @"
                    UPDATE public.""Gadget""
                    SET availability = false,
                        availability_date = @availabilityDate
                    WHERE gadget_id = @gadgetId";

                        using (var cmd = new NpgsqlCommand(gadgetUpdateSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("gadgetId", booking.GadgetId);
                            cmd.Parameters.AddWithValue("availabilityDate", booking.RentalEndDate.AddDays(1));
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        // If anything goes wrong, roll back both operations
                        await transaction.RollbackAsync();
                        throw; // Re-throw the exception to be handled by the caller
                    }
                }
            }
        }

        public async Task CreatePayment(Payment payment)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"INSERT INTO public.""Payment"" (payment_id, booking_id, amount, payment_method, payment_status, transaction_date)
                            VALUES (@paymentId, @bookingId, @amount, @paymentMethod, @paymentStatus, @transactionDate)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("paymentId", payment.PaymentId);
                    cmd.Parameters.AddWithValue("bookingId", payment.BookingId);
                    cmd.Parameters.AddWithValue("amount", payment.Amount);
                    cmd.Parameters.AddWithValue("paymentMethod", payment.PaymentMethod);
                    cmd.Parameters.AddWithValue("paymentStatus", payment.PaymentStatus);
                    cmd.Parameters.AddWithValue("transactionDate", payment.TransactionDate);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }




        //--------------------------- SEARCH SERVICES ---------------------------//
        public async Task<List<Gadget>> SearchGadgets(string searchQuery, string category = null, decimal? minPrice = null, decimal? maxPrice = null, float? minRating = null, int? minCondition = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var gadgets = new List<Gadget>();

                // Clean and prepare the search query
                searchQuery = searchQuery?.Trim() ?? "";

                var sql = @"
                        SELECT DISTINCT g.*, 
                               gi.image[1] as first_image,
                               COUNT(DISTINCT b.booking_id) as times_rented,
                               u.city as owner_city
                        FROM public.""Gadget"" g
                        LEFT JOIN public.""GadgetImages"" gi ON g.gadget_id = gi.gadget_id
                        LEFT JOIN public.""Booking"" b ON g.gadget_id = b.gadget_id
                        LEFT JOIN public.""User"" u ON g.owner_id = u.user_id
                        WHERE (@searchQuery = '' OR ( -- Only apply search if there's a search query
                            LOWER(g.title) LIKE LOWER(@searchQuery)
                            OR LOWER(g.description) LIKE LOWER(@searchQuery)
                            OR LOWER(g.brand) LIKE LOWER(@searchQuery)
                        ))
                        AND (@category IS NULL OR LOWER(g.category) = LOWER(@category))
                        AND (@minPrice IS NULL OR g.rental_price >= @minPrice)
                        AND (@maxPrice IS NULL OR g.rental_price <= @maxPrice)
                        AND (@minRating IS NULL OR g.gadget_rating >= @minRating)
                        AND (@minCondition IS NULL OR g.condition_metric >= @minCondition)
                        GROUP BY g.gadget_id, gi.image[1], u.city";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    // Add parameters with explicit types
                    cmd.Parameters.Add(new NpgsqlParameter("searchQuery", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = string.IsNullOrWhiteSpace(searchQuery) ? "" : $"%{searchQuery}%" });

                    cmd.Parameters.Add(new NpgsqlParameter("category", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = string.IsNullOrWhiteSpace(category) ? DBNull.Value : category.ToLower() });

                    cmd.Parameters.Add(new NpgsqlParameter("minPrice", NpgsqlTypes.NpgsqlDbType.Numeric)
                    { Value = (object)minPrice ?? DBNull.Value });

                    cmd.Parameters.Add(new NpgsqlParameter("maxPrice", NpgsqlTypes.NpgsqlDbType.Numeric)
                    { Value = (object)maxPrice ?? DBNull.Value });

                    cmd.Parameters.Add(new NpgsqlParameter("minRating", NpgsqlTypes.NpgsqlDbType.Real)
                    { Value = (object)minRating ?? DBNull.Value });

                    cmd.Parameters.Add(new NpgsqlParameter("minCondition", NpgsqlTypes.NpgsqlDbType.Integer)
                    { Value = (object)minCondition ?? DBNull.Value });

                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"Executing search with parameters:");
                    foreach (NpgsqlParameter p in cmd.Parameters)
                    {
                        System.Diagnostics.Debug.WriteLine($"Parameter {p.ParameterName}: {p.Value} (Type: {p.NpgsqlDbType})");
                    }

                    try
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var hasRows = reader.HasRows;
                            System.Diagnostics.Debug.WriteLine($"Query returned results: {hasRows}");

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
                                    Images = reader.IsDBNull(reader.GetOrdinal("first_image")) ? null : new[] { (byte[])reader["first_image"] },
                                    TimesRented = reader.GetInt32(reader.GetOrdinal("times_rented")),
                                    OwnerCity = reader.IsDBNull(reader.GetOrdinal("owner_city")) ? null : reader.GetString(reader.GetOrdinal("owner_city"))
                                };
                                gadgets.Add(gadget);
                                System.Diagnostics.Debug.WriteLine($"Found gadget: {gadget.Title}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error executing query: {ex.Message}");
                        throw;
                    }
                }
                return gadgets;
            }
        }



        //--------------------------- update gadget availability everytime the app launches ---------------------------//
        public async Task UpdateGadgetAvailabilityStatus()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                            -- Update gadgets that have passed their availability_date to be today
                            UPDATE public.""Gadget"" g
                            SET 
                                availability = true,
                                availability_date = CURRENT_DATE
                            WHERE availability_date IS NOT NULL 
                            AND CURRENT_DATE > availability_date;

                            -- Then, update gadgets based on current bookings
                            UPDATE public.""Gadget"" g
                            SET 
                                availability = NOT EXISTS (
                                    SELECT 1 
                                    FROM public.""Booking"" b
                                    WHERE b.gadget_id = g.gadget_id
                                    AND CURRENT_DATE BETWEEN b.rental_start_date AND b.rental_end_date
                                ),
                                availability_date = (
                                    SELECT b.rental_end_date + 1
                                    FROM public.""Booking"" b
                                    WHERE b.gadget_id = g.gadget_id
                                    AND CURRENT_DATE BETWEEN b.rental_start_date AND b.rental_end_date
                                )
                            WHERE EXISTS (
                                SELECT 1 
                                FROM public.""Booking"" b
                                WHERE b.gadget_id = g.gadget_id
                                AND CURRENT_DATE BETWEEN b.rental_start_date AND b.rental_end_date
                            );";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}