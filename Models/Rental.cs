using System;

namespace Models
{
    public class Rental
    {
        public int Id { get; set; } // ID (unikāls identifikators)
        public int CarId { get; set; } // Car being rented
        public int CustomerId { get; set; } // Customer renting the car
        public DateTime StartTime { get; set; } // Īres sākuma laiks
        public DateTime? EndTime { get; set; } // Īres beigu laiks
        public decimal? KilometersDriven { get; set; } // Nobraukto kilometru skaits
        public decimal? TotalPayment { get; set; } // Maksājuma kopsumma
    }
}