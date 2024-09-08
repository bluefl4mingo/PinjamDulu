using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string ContactInformation { get; set; }
        public int ProfilePicture { get; set; }

        private readonly BookList _bookList;

        public User(string name)
        {
            Name = name;

            _bookList = new BookList();
        }

        public IEnumerable<Booking> GetBookingForUser(string userId)
        {
            return _bookList.GetBookingForUser(userId);
        }

        public void MakeBooking(Booking booking)
        {
            _bookList.AddBooking(booking);
        }
    }
}
