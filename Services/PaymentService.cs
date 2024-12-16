using Models;

public class PaymentService
{
    public decimal CalculatePayment(Rental rental, Car car)
    {
        var durationHours = (decimal)(rental.EndTime.Value - rental.StartTime).TotalHours;
        var payment = (durationHours * car.HourlyRate) + (rental.KilometersDriven.Value * car.KilometerRate);
        return payment;
    }
}