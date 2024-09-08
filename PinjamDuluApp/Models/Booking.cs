using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    public class Booking
    {
        public string Id { get; set; }
        public string GadgetID { get; set; }
        public string LenderID { get; set; }
        public string RenterID { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime RentStartDate { get; set; }
        public DateTime RentEndDate { get; set; }
        public TimeSpan RentDuraion => RentStartDate.Subtract(RentEndDate);
        public int Price { get; set; }
        public bool PaymentStatus { get; set; }

        public Booking(string id, DateTime rentStartDate, DateTime rentEndDate)
        {
            Id = id;
            RentStartDate = rentStartDate;
            RentEndDate = rentEndDate;
        }

        public bool Conflicts(Booking booking)
        {
            return booking.RentStartDate < RentEndDate && booking.RentEndDate > RentStartDate;
        } 
    }
}
