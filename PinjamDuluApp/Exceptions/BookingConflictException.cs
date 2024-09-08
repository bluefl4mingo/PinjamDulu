using PinjamDuluApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Exceptions
{
    public class BookingConflictException : Exception
    {
        public Booking ExistingBooking { get; }
        public Booking IncomingBooking { get; }

        public BookingConflictException(Booking existingBooking, Booking incomingBooking)
        {
            ExistingBooking = existingBooking;
            IncomingBooking = incomingBooking;
        }

        public BookingConflictException(string? message, Booking existingBooking, Booking incomingBooking) : base(message)
        {
            ExistingBooking = existingBooking;
            IncomingBooking = incomingBooking;
        }

        public BookingConflictException(string? message, Exception? innerException, Booking existingBooking, Booking incomingBooking) : base(message, innerException)
        {
            ExistingBooking = existingBooking;
            IncomingBooking = incomingBooking;
        }

        protected BookingConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
