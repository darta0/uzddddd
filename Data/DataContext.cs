using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Models;

public class DataContext
{
    private SqliteConnection _connection;

    public DataContext(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
    }

    public void InitializeDatabase()
    {
        _connection.Open();
        // Create tables if not exist
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cars (
                    Id INTEGER PRIMARY KEY,
                    Model TEXT,
                    HourlyRate REAL,
                    KilometerRate REAL
                );
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY,
                    FullName TEXT,
                    Email TEXT
                );
                CREATE TABLE IF NOT EXISTS Rentals (
                    Id INTEGER PRIMARY KEY,
                    CarId INTEGER,
                    CustomerId INTEGER,
                    StartTime TEXT,
                    EndTime TEXT,
                    KilometersDriven REAL,
                    TotalPayment REAL,
                    FOREIGN KEY(CarId) REFERENCES Cars(Id),
                    FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
                );";
            command.ExecuteNonQuery();
        }
    }

    public void AddCustomer(Customer customer)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Customers (FullName, Email) VALUES (@FullName, @Email)";
            command.Parameters.AddWithValue("@FullName", customer.FullName);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.ExecuteNonQuery();
        }
    }

    public List<Car> GetAvailableCars()
    {
        var cars = new List<Car>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM Cars";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var car = new Car
                    {
                        Id = reader.GetInt32(0),
                        Model = reader.GetString(1),
                        HourlyRate = reader.GetDecimal(2),
                        KilometerRate = reader.GetDecimal(3)
                    };
                    cars.Add(car);
                }
            }
        }
        return cars;
    }

    public void AddRental(Rental rental)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"INSERT INTO Rentals 
                (CarId, CustomerId, StartTime, EndTime, KilometersDriven, TotalPayment) 
                VALUES (@CarId, @CustomerId, @StartTime, @EndTime, @KilometersDriven, @TotalPayment)";
            command.Parameters.AddWithValue("@CarId", rental.CarId);
            command.Parameters.AddWithValue("@CustomerId", rental.CustomerId);
            command.Parameters.AddWithValue("@StartTime", rental.StartTime);
            command.Parameters.AddWithValue("@EndTime", rental.EndTime);
            command.Parameters.AddWithValue("@KilometersDriven", rental.KilometersDriven);
            command.Parameters.AddWithValue("@TotalPayment", rental.TotalPayment);
            command.ExecuteNonQuery();
        }
    }

    public void CompleteRental(int rentalId, decimal kilometersDriven)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"UPDATE Rentals 
                SET KilometersDriven = @KilometersDriven, EndTime = @EndTime 
                WHERE Id = @RentalId";
            command.Parameters.AddWithValue("@KilometersDriven", kilometersDriven);
            command.Parameters.AddWithValue("@EndTime", DateTime.Now);
            command.Parameters.AddWithValue("@RentalId", rentalId);
            command.ExecuteNonQuery();
        }
    }

    public Rental GetRentalById(int rentalId)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM Rentals WHERE Id = @RentalId";
            command.Parameters.AddWithValue("@RentalId", rentalId);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Rental
                    {
                        Id = reader.GetInt32(0),
                        CarId = reader.GetInt32(1),
                        CustomerId = reader.GetInt32(2),
                        StartTime = DateTime.Parse(reader.GetString(3)),
                        EndTime = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4)),
                        KilometersDriven = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                        TotalPayment = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6)
                    };
                }
            }
        }
        return null;
    }

    public Car GetCarById(int carId)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM Cars WHERE Id = @CarId";
            command.Parameters.AddWithValue("@CarId", carId);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Car
                    {
                        Id = reader.GetInt32(0),
                        Model = reader.GetString(1),
                        HourlyRate = reader.GetDecimal(2),
                        KilometerRate = reader.GetDecimal(3)
                    };
                }
            }
        }
        return null;
    }

    public void CompleteRental(int rentalId, decimal kilometersDriven, decimal totalPayment)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Rentals 
                SET KilometersDriven = @KilometersDriven, 
                    TotalPayment = @TotalPayment,
                    EndTime = @EndTime 
                WHERE Id = @RentalId";
            command.Parameters.AddWithValue("@KilometersDriven", kilometersDriven);
            command.Parameters.AddWithValue("@TotalPayment", totalPayment);
            command.Parameters.AddWithValue("@EndTime", DateTime.Now);
            command.Parameters.AddWithValue("@RentalId", rentalId);
            command.ExecuteNonQuery();
        }
    }
}