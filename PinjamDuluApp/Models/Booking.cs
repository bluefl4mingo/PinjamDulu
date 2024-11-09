using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    public class Booking
    {
        public Guid BookingId { get; set; }
        public Guid GadgetId { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid LenderId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }

    }
}
