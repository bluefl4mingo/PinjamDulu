using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinjamDuluApp.Models
{
    internal class Gadget
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Condition { get; set; }
        public int RentalPrice { get; set; }
        public bool Availability { get; set; }
        public DateTime AvailabilityDate {  get; set; }
        public int Images { get; set; }

        public Gadget(string title, string desctiption)
        {
            Title = title;
            Description = desctiption;
        }
    }
}
