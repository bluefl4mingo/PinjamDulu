using System;

namespace PinjamDuluApp.Models
{
    public class Gadget
    {
        public Guid GadgetId { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public int ConditionMetric { get; set; }
        public float GadgetRating { get; set; }
        public decimal RentalPrice { get; set; }
        public bool Availability { get; set; }
        public DateTime? AvailabilityDate { get; set; }
        public byte[][] Images { get; set; }
        // Additional properties for UI
        public int TimesRented { get; set; }
        public string CurrentRenterUsername { get; set; }
        public DateTime? CurrentRentalStart { get; set; }
        public DateTime? CurrentRentalEnd { get; set; }
        public string OwnerCity { get; set; }
        public string OwnerName { get; set; }  // Added this property
    }
}