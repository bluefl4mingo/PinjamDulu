using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    internal class Booking
    {
        public string Id { get; set; }
        public string GadgetID { get; set; }
        public string LenderID { get; set; }
        public string RenterID { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime RentStartDate { get; set; }
        public DateTime RentEndDate { get; set; }
        public int Price { get; set; }
        public bool PaymentStatus { get; set; }

        public Booking()
        {

        }
    }
}
