namespace Models
{
    public class Car
    {
        public int Id { get; set; } // ID (unikāls identifikators)
        public string? Model { get; set; } // Modelis (piem., Model 3, Model Y)
        public decimal HourlyRate { get; set; } // Stundas likme (EUR/h)
        public decimal KilometerRate { get; set; } // Kilometra likme (EUR/km)
    }
}