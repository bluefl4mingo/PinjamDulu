using PinjamDuluApp.Models;
using System;

namespace PinjamDuluApp.Models
{
    public class RentalItem
    {
        public Guid BookingId { get; set; }
        public Gadget Gadget { get; set; }
        public string OwnerName { get; set; }
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
        public Review Review { get; set; }

        public bool IsCompleteRentVisible => RentalEndDate <= DateTime.Now && Review == null;
        public bool IsReviewVisible => Review != null;
    }
}