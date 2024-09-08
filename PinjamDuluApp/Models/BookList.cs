using PinjamDuluApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    public class BookList
    {
        private readonly List<Booking> _bookings;

        public BookList()
        {
            _bookings = new List<Booking>();
        }

        public IEnumerable<Booking> GetBookingForUser(string userId)
        {
            return _bookings.Where(r => r.RenterID  == userId);
        }

        public void AddBooking(Booking booking)
        {
            foreach (Booking existingBooking in _bookings)
            {
                if(existingBooking.Conflicts(booking))
                {
                    throw new BookingConflictException(existingBooking, booking);
                }
            }

            _bookings.Add(booking);
        }
    }
}
