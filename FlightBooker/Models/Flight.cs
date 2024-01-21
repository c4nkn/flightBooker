namespace FlightBooker.Models;

public class Flight
{
    public int Id { get; set; }
    public string? FlightNumber { get; set; }
    public string? Airline { get; set; }
    
    // Aircraft Information
    public Aircraft Aircraft { get; set; } = new Aircraft();
    public List<string> AvailableSeats { get; set; } = new List<string>();
    public List<string> TakenSeats { get; set; } = new List<string>();
    
    // Airport Information
    public Airport Departure { get; set; } = new Airport();
    public Airport Destination { get; set; } = new Airport();
    
    // Time Information
    public string FlightDate { get; set; }
    public string FlightTime { get; set; }
    public string DepartureTime { get; set; }
    public string ArrivalTime { get; set; }
    
    public decimal TotalPrice { get; set; }
}