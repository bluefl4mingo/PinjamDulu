using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    internal class Payment
    {
        public string Id { get; set; }
        public string BookingID { get; set; }
        public int Amount { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime TransactionDate { get; set; }

        public Payment() 
        { 
        
        }
    }
}
