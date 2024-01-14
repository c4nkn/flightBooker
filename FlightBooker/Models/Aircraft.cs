namespace FlightBooker.Models;

public class Aircraft
{
    public int Id { get; set; }
    public string? AircraftType { get; set; }
    public string? TypeCode { get; set; }
    public string? Manufacturer { get; set; }
    public int Capacity { get; set; }
}