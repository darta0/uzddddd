using System;
using Microsoft.Data.Sqlite;

class Program
{
    public static void Main()
    {
        string connectionString = "Data Source=tesla.db";
    
        try 
        {
            Console.WriteLine("Welcome to the application of renting Tesla cars!");
            var teslaCtrl = new TeslaCtrl(connectionString);
            teslaCtrl.AddTesla("Model 3", "12.35", "0.30");
            teslaCtrl.AddTesla("Model Y", "25.19", "0.50");
            teslaCtrl.AddTesla("Model S", "17.81", "0.40");
            
            while (true)
            {
                Console.WriteLine("Choose an action:\n'register' - register yourself,\n'start' - start a ride,\n'stop' - stop the ride,\n'print' - show all available Teslas,\n'exit' - close the program.");
                var userCommand = Console.ReadLine();
                
                switch (userCommand)
                {
                    case "register":
                        teslaCtrl.AddClient();
                        break;
                    case "start":
                        teslaCtrl.StartRent();
                        break;
                    case "stop":
                        teslaCtrl.StopRent();
                        break;
                    case "print":
                        teslaCtrl.PrintTeslas();
                        break;
                    case "exit":
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);    
        }
    }
    
    public class TeslaCtrl
    {
        private readonly string connectionString;
        private int clientId;
        private int currentRentId;
        
        public TeslaCtrl(string connectionString)
        {
            this.connectionString = connectionString;
            CreateTeslaTable();
            CreateClientTable();
            CreateRentTable();
        }
        
        private void CreateTeslaTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = 
                    @"CREATE TABLE IF NOT EXISTS Teslas (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Model TEXT NOT NULL,
                        HourlyRate REAL NOT NULL,
                        KilometerRate REAL NOT NULL
                        );";
                createTableCmd.ExecuteNonQuery();
            }
        }
        
        private void CreateClientTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = 
                    @"CREATE TABLE IF NOT EXISTS Clients (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Surname TEXT NOT NULL,
                        Email TEXT NOT NULL
                        );";
                createTableCmd.ExecuteNonQuery();
            }
        }

        private void CreateRentTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = 
                    @"CREATE TABLE IF NOT EXISTS Rents (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        StartDate DATETIME NOT NULL,
                        FinishDate DATETIME,
                        DurationMinutes INTEGER,
                        Kilometers REAL,
                        Price REAL,
                        CarID INTEGER NOT NULL,
                        ClientID INTEGER NOT NULL,
                        FOREIGN KEY (CarID) REFERENCES Teslas(ID),
                        FOREIGN KEY (ClientID) REFERENCES Clients(ID)
                        );";
                createTableCmd.ExecuteNonQuery();
            }
        }
        
        public void AddClient()
        {
            Console.WriteLine("Please, enter your name:");
            string clientName = Console.ReadLine();
            
            Console.WriteLine("Please, enter your surname:");
            string clientSurname = Console.ReadLine();
            
            Console.WriteLine("Please, enter your e-mail:");
            string clientMail = Console.ReadLine();
            
            if (string.IsNullOrEmpty(clientName) || string.IsNullOrEmpty(clientSurname) || string.IsNullOrEmpty(clientMail))
            {
                Console.WriteLine("All fields are required.");
                return;
            }
            
            AddClientToTable(clientName, clientSurname, clientMail);
        }

        private void AddClientToTable(string name, string surname, string mail)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Clients(Name, Surname, Email) VALUES (@name, @surname, @mail)";
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@surname", surname);
                insertCmd.Parameters.AddWithValue("@mail", mail);

                insertCmd.ExecuteNonQuery();
                
                var getIdCmd = connection.CreateCommand();
                getIdCmd.CommandText = "SELECT last_insert_rowid()";
                clientId = Convert.ToInt32(getIdCmd.ExecuteScalar());
            }
        }

        public void AddTesla(string model, string hourlyrate, string kilometerrate)
        {
            AddTeslaToTable(model, hourlyrate, kilometerrate);
        }
        
        private void AddTeslaToTable(string model, string hourlyrate, string kilometerrate)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Teslas(Model, HourlyRate, KilometerRate) VALUES (@model, @hourlyrate, @kilometerrate)";
                insertCmd.Parameters.AddWithValue("@model", model);
                insertCmd.Parameters.AddWithValue("@hourlyrate", hourlyrate);
                insertCmd.Parameters.AddWithValue("@kilometerrate", kilometerrate);

                insertCmd.ExecuteNonQuery();
            } 
        }

        public void StartRent()
        {
            Console.WriteLine("Choose a car from the available options below:");
            PrintTeslas();  
            
            Console.WriteLine("Enter the ID of the car you are selecting:");
            int carId = Convert.ToInt32(Console.ReadLine());
            AddRentToTable(carId, clientId);
        }

        private void AddRentToTable(int carId, int clientId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
            
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO Rents(StartDate, CarID, ClientID) VALUES (@startdate, @carid, @clientid)";
                insertCmd.Parameters.AddWithValue("@startdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertCmd.Parameters.AddWithValue("@carid", carId);
                insertCmd.Parameters.AddWithValue("@clientid", clientId);

                insertCmd.ExecuteNonQuery();
                Console.WriteLine("Rent started.");
                Console.WriteLine($"Start Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT last_insert_rowid()";
                currentRentId = Convert.ToInt32(selectCmd.ExecuteScalar());
            } 
        }
        
        public void StopRent()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
        
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Rents WHERE ID = @rentId";
                selectCmd.Parameters.AddWithValue("@rentId", currentRentId);
        
                Console.WriteLine("Enter the kilometers driven during the rent:");
                double kilometersDriven = Convert.ToDouble(Console.ReadLine());
                
                DateTime finishDate = DateTime.Now;
                
                double durationInMinutes;
                double price = CalculateRentPrice(currentRentId, kilometersDriven, finishDate, out durationInMinutes);
                
                UpdateRent(currentRentId, finishDate, kilometersDriven, price, durationInMinutes);
            }
        }
        
        private double CalculateRentPrice(int rentId, double kilometersDriven, DateTime finishDate, out double durationInMinutes)
        {
            double price = 0;
            durationInMinutes = 0;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT Rents.StartDate, Teslas.HourlyRate, Teslas.KilometerRate FROM Rents " +
                                        "JOIN Teslas ON Rents.CarID = Teslas.ID WHERE Rents.ID = @rentId";
                selectCmd.Parameters.AddWithValue("@rentId", rentId);

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
                        double hourlyRate = Convert.ToDouble(reader["HourlyRate"]);
                        double kilometerRate = Convert.ToDouble(reader["KilometerRate"]);

                        durationInMinutes = Math.Round((finishDate - startDate).TotalMinutes, 2);

                        price = (durationInMinutes / 60) * hourlyRate + kilometersDriven * kilometerRate;
                        price = Math.Round(price, 2);
                    }
                }
            }
            
            return price;
        }
        
        private void UpdateRent(int rentId, DateTime finishDate, double kilometersDriven, double price, double durationInMinutes)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Rents SET FinishDate = @finishDate, Kilometers = @kilometers, Price = @price, DurationMinutes = @duration " +
                                        "WHERE ID = @rentId";
                updateCmd.Parameters.AddWithValue("@finishDate", finishDate.ToString("yyyy-MM-dd HH:mm:ss"));
                updateCmd.Parameters.AddWithValue("@kilometers", kilometersDriven);
                updateCmd.Parameters.AddWithValue("@price", price);
                updateCmd.Parameters.AddWithValue("@duration", durationInMinutes);
                updateCmd.Parameters.AddWithValue("@rentId", rentId);

                updateCmd.ExecuteNonQuery();
                Console.WriteLine($"Rent stopped.\nFinish Date: {finishDate}.\nDuration: {durationInMinutes} min. Price: {price} EUR.");
            }
        }
        
        public void PrintTeslas()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Teslas";

                using (var reader = selectCmd.ExecuteReader())
                {                
                    while(reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["ID"]}, model: {reader["Model"]}, price per hour: {reader["HourlyRate"]} EUR, price per kilometer: {reader["KilometerRate"]} EUR.");
                    }
                }
            }
        }
    }
}
