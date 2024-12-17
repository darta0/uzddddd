using System;
using Microsoft.Data.Sqlite;

class TeslaRentalApp
{
    public static void Main()
    {
        var dbConnection = "Data Source=tesla.db";
        
        try
        {
            Console.WriteLine("Welcome to the Tesla rental service!");
            var rentalManager = new RentalManager(dbConnection);
            rentalManager.AddTesla("Model 3", "12.35", "0.30");
            rentalManager.AddTesla("Model Y", "25.19", "0.50");
            rentalManager.AddTesla("Model S", "17.81", "0.40");
            
            while (true)
            {
                Console.WriteLine("Select an option: 'register' - sign up, 'start' - start a ride, 'stop' - end the ride, 'list' - show available Teslas, 'exit' - close the app.");
                var action = Console.ReadLine();
                
                switch (action)
                {
                    case "register":
                        rentalManager.RegisterClient();
                        break;
                    case "start":
                        rentalManager.BeginRental();
                        break;
                    case "stop":
                        rentalManager.EndRental();
                        break;
                    case "list":
                        rentalManager.ShowTeslas();
                        break;
                    case "exit":
                        return;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);    
        }
    }
    
    public class RentalManager
    {
        private readonly string _connectionString;
        private int _currentClientId;
        private int _activeRentId;
        
        public RentalManager(string connectionString)
        {
            _connectionString = connectionString;
            SetupDatabaseTables();
        }
        
        private void SetupDatabaseTables()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                
                string[] createTableCommands = new string[]
                {
                    @"CREATE TABLE IF NOT EXISTS Cars (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Model TEXT NOT NULL,
                        HourlyRate REAL NOT NULL,
                        KilometerRate REAL NOT NULL
                    );",
                    @"CREATE TABLE IF NOT EXISTS Customers (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Surname TEXT NOT NULL,
                        Email TEXT NOT NULL
                    );",
                    @"CREATE TABLE IF NOT EXISTS Rentals (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        StartTime DATETIME NOT NULL,
                        EndTime DATETIME,
                        DurationMinutes INTEGER,
                        Distance REAL,
                        Cost REAL,
                        CarID INTEGER NOT NULL,
                        CustomerID INTEGER NOT NULL,
                        FOREIGN KEY (CarID) REFERENCES Cars(ID),
                        FOREIGN KEY (CustomerID) REFERENCES Customers(ID)
                    );"
                };
                
                foreach (var command in createTableCommands)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public void RegisterClient()
        {
            Console.WriteLine("Enter your first name:");
            var firstName = Console.ReadLine();
            
            Console.WriteLine("Enter your last name:");
            var lastName = Console.ReadLine();
            
            Console.WriteLine("Enter your email address:");
            var email = Console.ReadLine();
            
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
            {
                Console.WriteLine("All fields are mandatory.");
                return;
            }
            
            AddClientToDatabase(firstName, lastName, email);
        }

        private void AddClientToDatabase(string name, string surname, string email)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Customers(Name, Surname, Email) VALUES (@name, @surname, @email)";
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@surname", surname);
                insertCmd.Parameters.AddWithValue("@email", email);

                insertCmd.ExecuteNonQuery();
                
                var getIdCmd = connection.CreateCommand();
                getIdCmd.CommandText = "SELECT last_insert_rowid()";
                _currentClientId = Convert.ToInt32(getIdCmd.ExecuteScalar());
            }
        }

        public void AddTesla(string model, string hourlyRate, string kilometerRate)
        {
            InsertTeslaIntoDatabase(model, hourlyRate, kilometerRate);
        }
        
        private void InsertTeslaIntoDatabase(string model, string hourlyRate, string kilometerRate)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Cars(Model, HourlyRate, KilometerRate) VALUES (@model, @hourlyRate, @kilometerRate)";
                insertCmd.Parameters.AddWithValue("@model", model);
                insertCmd.Parameters.AddWithValue("@hourlyRate", hourlyRate);
                insertCmd.Parameters.AddWithValue("@kilometerRate", kilometerRate);

                insertCmd.ExecuteNonQuery();
            } 
        }

        public void BeginRental()
        {
            Console.WriteLine("Available Teslas: ");
            ShowTeslas();  
            
            Console.WriteLine("Select a car by entering its ID:");
            int selectedCarId = Convert.ToInt32(Console.ReadLine());
            StartRentalSession(selectedCarId, _currentClientId);
        }

        private void StartRentalSession(int carId, int clientId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Rentals(StartTime, CarID, CustomerID) VALUES (@startTime, @carId, @customerId)";
                insertCmd.Parameters.AddWithValue("@startTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertCmd.Parameters.AddWithValue("@carId", carId);
                insertCmd.Parameters.AddWithValue("@customerId", clientId);

                insertCmd.ExecuteNonQuery();
                Console.WriteLine("Rental started.");
                Console.WriteLine($"Start Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT last_insert_rowid()";
                _activeRentId = Convert.ToInt32(selectCmd.ExecuteScalar());
            } 
        }
        
        public void EndRental()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
        
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Rentals WHERE ID = @rentId";
                selectCmd.Parameters.AddWithValue("@rentId", _activeRentId);
        
                Console.WriteLine("Enter the kilometers traveled:");
                double kilometersDriven = Convert.ToDouble(Console.ReadLine());
                
                DateTime rentalEndTime = DateTime.Now;
                
                double rentalDuration;
                double rentalCost = CalculateRentalCost(_activeRentId, kilometersDriven, rentalEndTime, out rentalDuration);
                
                UpdateRentalSession(_activeRentId, rentalEndTime, kilometersDriven, rentalCost, rentalDuration);
            }
        }
        
        private double CalculateRentalCost(int rentId, double kilometersDriven, DateTime rentalEndTime, out double rentalDuration)
        {
            double cost = 0;
            rentalDuration = 0;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT Rentals.StartTime, Cars.HourlyRate, Cars.KilometerRate FROM Rentals " +
                                        "JOIN Cars ON Rentals.CarID = Cars.ID WHERE Rentals.ID = @rentId";
                selectCmd.Parameters.AddWithValue("@rentId", rentId);

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime rentalStartTime = Convert.ToDateTime(reader["StartTime"]);
                        double hourlyRate = Convert.ToDouble(reader["HourlyRate"]);
                        double kilometerRate = Convert.ToDouble(reader["KilometerRate"]);

                        rentalDuration = Math.Round((rentalEndTime - rentalStartTime).TotalMinutes, 2);

                        cost = (rentalDuration / 60) * hourlyRate + kilometersDriven * kilometerRate;
                        cost = Math.Round(cost, 2);
                    }
                }
            }
            
            return cost;
        }
        
        private void UpdateRentalSession(int rentId, DateTime rentalEndTime, double kilometersDriven, double cost, double rentalDuration)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Rentals SET EndTime = @endTime, Distance = @distance, Cost = @cost, DurationMinutes = @duration " +
                                        "WHERE ID = @rentId";
                updateCmd.Parameters.AddWithValue("@endTime", rentalEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
                updateCmd.Parameters.AddWithValue("@distance", kilometersDriven);
                updateCmd.Parameters.AddWithValue("@cost", cost);
                updateCmd.Parameters.AddWithValue("@duration", rentalDuration);
                updateCmd.Parameters.AddWithValue("@rentId", rentId);

                updateCmd.ExecuteNonQuery();
                Console.WriteLine($"Rental ended.\nEnd Time: {rentalEndTime}.\nDuration: {rentalDuration} min. Total Cost: {cost} EUR.");
            }
        }
        
        public void ShowTeslas()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Cars";

                using (var reader = selectCmd.ExecuteReader())
                {                
                    while(reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["ID"]}, Model: {reader["Model"]}, Hourly Rate: {reader["HourlyRate"]} EUR, Kilometer Rate: {reader["KilometerRate"]} EUR.");
                    }
                }
            }
        }
    }
}
