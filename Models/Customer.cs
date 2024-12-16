namespace Models
{
    public class Customer
    {
        public int Id { get; set; } // ID (unikāls identifikators)
        public string? FullName { get; set; } // Vārds un uzvārds
        public string? Email { get; set; } // E-pasta adrese
    }
}