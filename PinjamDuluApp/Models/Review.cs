using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    internal class Review
    {
        public string Id { get; set; }
        public string GadgetID { get; set; }
        public string LenderID { get; set; }
        public string RenterID { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }

        public Review()
        {

        }

    }
}
