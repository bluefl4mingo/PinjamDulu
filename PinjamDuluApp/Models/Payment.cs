using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
