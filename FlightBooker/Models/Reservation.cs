namespace FlightBooker.Models;

public class Booking
{
    public int Id { get; set; }
    public Flight SelectedFlight { get; set; }
    public string PassengerName { get; set; }
    public string PassengerSurname { get; set; }
    public string PassengerAge { get; set; }
    public string SelectedSeat { get; set; }
    public string ReservationCode { get; set; }
}