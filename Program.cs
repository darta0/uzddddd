using System;
using Models; // Added to recognize Customer and Rental classes

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Tesla Car Rental Platform!");
        // Updated connection string without 'Version'
        var connectionString = "Data Source=tesla_rental.db;";
        var dataContext = new DataContext(connectionString);
        dataContext.InitializeDatabase();

        // Display menu options and handle user input
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Register Customer");
            Console.WriteLine("2. List Available Cars");
            Console.WriteLine("3. Rent a Car");
            Console.WriteLine("4. Complete Rental");
            Console.WriteLine("5. Exit");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    // Register Customer
                    Console.Write("Enter full name: ");
                    var fullName = Console.ReadLine();
                    Console.Write("Enter email address: ");
                    var email = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                    {
                        Console.WriteLine("Full name and email cannot be empty.");
                        break;
                    }

                    var customer = new Customer { FullName = fullName, Email = email };
                    dataContext.AddCustomer(customer);
                    Console.WriteLine("Customer registered successfully.");
                    break;
                
                case "2":
                    // List Available Cars
                    var cars = dataContext.GetAvailableCars();
                    Console.WriteLine("Available Cars:");
                    foreach (var car in cars)
                    {
                        Console.WriteLine($"ID: {car.Id}, Model: {car.Model}, Hourly Rate: {car.HourlyRate} EUR/h, Kilometer Rate: {car.KilometerRate} EUR/km");
                    }
                    break;
                
                case "3":
                    // Rent a Car
                    Console.Write("Enter Customer ID: ");
                    var customerIdInput = Console.ReadLine();
                    if (!int.TryParse(customerIdInput, out int customerId))
                    {
                        Console.WriteLine("Invalid Customer ID.");
                        break;
                    }

                    Console.Write("Enter Car ID to rent: ");
                    var carIdInput = Console.ReadLine();
                    if (!int.TryParse(carIdInput, out int carId))
                    {
                        Console.WriteLine("Invalid Car ID.");
                        break;
                    }

                    var rental = new Rental
                    {
                        CarId = carId,
                        CustomerId = customerId,
                        StartTime = DateTime.Now
                    };
                    dataContext.AddRental(rental);
                    Console.WriteLine("Car rented successfully.");
                    break;
                
                case "4":
                    // Complete Rental
                    Console.Write("Enter Rental ID: ");
                    var rentalIdInput = Console.ReadLine();
                    if (!int.TryParse(rentalIdInput, out int rentalId))
                    {
                        Console.WriteLine("Invalid Rental ID.");
                        break;
                    }

                    Console.Write("Enter kilometers driven: ");
                    var kmInput = Console.ReadLine();
                    if (!decimal.TryParse(kmInput, out decimal kilometersDriven))
                    {
                        Console.WriteLine("Invalid kilometers driven.");
                        break;
                    }

                    var rentalObj = dataContext.GetRentalById(rentalId);
                    if (rentalObj == null)
                    {
                        Console.WriteLine("Rental not found.");
                        break;
                    }

                    var carObj = dataContext.GetCarById(rentalObj.CarId);
                    if (carObj == null)
                    {
                        Console.WriteLine("Car not found.");
                        break;
                    }

                    var paymentService = new PaymentService();
                    var totalPayment = paymentService.CalculatePayment(rentalObj, carObj);

                    dataContext.CompleteRental(rentalId, kilometersDriven, totalPayment);
                    Console.WriteLine($"Rental completed. Total payment: {totalPayment} EUR.");
                    break;
                
                case "5":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}