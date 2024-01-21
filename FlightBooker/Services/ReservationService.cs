using FlightBooker.Models;
using Spectre.Console;

namespace FlightBooker.Services;

public class ReservationService
{
    public static void CreateReservation()
    {
        CLIService.DisplayHeader("Location", "round_pushpin");
        var (departureInput, arrivalInput) = GetLocations();
            
        var filteredFlights = DataService.FilterFlights(departureInput, arrivalInput);

        if (filteredFlights != null && filteredFlights.Count > 0)
        {
            CLIService.DisplayHeader("Available Flights", "airplane_departure");
            CLIService.DisplayFlightsTable(filteredFlights, departureInput, arrivalInput);
                
            Console.Write("Enter flight number: ");
            var flightNumberInput = Console.ReadLine();
            
            Flight targetFlight = DataService.GetFlightByFlightNumber(flightNumberInput);

            if (targetFlight != null)
            {
                int rows = targetFlight.Aircraft.Capacity / 6;
                List<string> takenSeats = targetFlight.TakenSeats;
                string selectedSeat = CLIService.DisplaySeats(rows, takenSeats);
                
                Console.Write("\nEnter passenger name: ");
                var passengerName = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(passengerName))
                {
                    Console.WriteLine("Invalid passenger name. Please try again.");
                    Console.Write("\nEnter passenger name: ");
                    passengerName = Console.ReadLine();
                    return;
                }
                
                Console.Write("\nEnter passenger surname: ");
                var passengerSurname = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(passengerSurname))
                {
                    Console.WriteLine("Invalid passenger surname. Please try again.");
                    Console.Write("\nEnter passenger surname: ");
                    passengerSurname = Console.ReadLine();
                    return;
                }
                
                CLIService.DisplayHeader("Summary", "spiral_notepad");
                if (CLIService.ShowReservationSummary(targetFlight, selectedSeat, passengerName, passengerSurname))
                {
                    targetFlight.AvailableSeats.Remove(selectedSeat);
                    targetFlight.TakenSeats.Add(selectedSeat);
                    DataService.UpdateFlight(targetFlight);
                    CLIService.GoBack();
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Flight with '{flightNumberInput}' flight number not found.[/]");
                CreateReservation();
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No flights found for the given departure and arrival cities.[/]");
            CreateReservation();
        }
    }

    public static (string, string) GetLocations()
    {
        Console.Write("Departure (ie. Istanbul): ");
        var departureInput = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(departureInput))
        {
            Console.WriteLine("Invalid city name. Please enter a valid city name.");
            Console.Write("Departure (ie. Istanbul): ");
            departureInput = Console.ReadLine();
        }
            
        Console.Write("Arrival (ie. Giresun): ");
        var arrivalInput = Console.ReadLine();
            
        while (string.IsNullOrWhiteSpace(arrivalInput))
        {
            Console.WriteLine("Invalid city name. Please enter a valid city name.");
            Console.Write("Arrival (ie. Giresun): ");
            arrivalInput = Console.ReadLine();
        }

        return (departureInput, arrivalInput);
    }

    public static void CancelReservation(string pnrCode)
    {
        var reservations = DataService.ReadJson(1);

        if (reservations == null)
        {
            Console.WriteLine("Error reading reservations data or the data is null.");
            return;
        }

        try
        {
            var reservationToRemove = reservations.FirstOrDefault(r => r["PNRCode"]?.ToString().Equals(pnrCode, StringComparison.OrdinalIgnoreCase) ?? false);

            if (reservationToRemove != null)
            {
                reservations.Remove(reservationToRemove);
                DataService.SaveJson(1, reservations);
                
                var flightNumber = reservationToRemove["SelectedFlight"]?["FlightNumber"]?.ToString();
                var targetFlight = DataService.GetFlightByFlightNumber(flightNumber);

                if (targetFlight != null)
                {
                    var selectedSeat = reservationToRemove["SelectedSeat"]?.ToString();
                    targetFlight.TakenSeats.Remove(selectedSeat);
                    DataService.UpdateFlight(targetFlight);
                    targetFlight.AvailableSeats.Add(selectedSeat);
                    DataService.UpdateFlight(targetFlight);
                }

                Console.WriteLine($"Reservation with PNR code {pnrCode} has been canceled.");
            }
            else
            {
                Console.WriteLine($"No reservation found with PNR code {pnrCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling reservation: {ex.Message}");
        }
    }

    public static void ReservationInformation(string pnrCode)
    {
        var reservations = DataService.ReadJson(1);
        
        if (reservations == null)
        {
            Console.WriteLine("Error reading reservations data or the data is null.");
            return;
        }

        try
        {
            var reservation = reservations.FirstOrDefault(r => r["PNRCode"]?.ToString().Equals(pnrCode, StringComparison.OrdinalIgnoreCase) ?? false);
            
            var flightNumber = reservation["SelectedFlight"]?["FlightNumber"]?.ToString();
            var airline = reservation["SelectedFlight"]?["Airline"]?.ToString();
            var aircraftType = reservation["SelectedFlight"]?["Aircraft"]?["AircraftType"]?.ToString();
            var departureTime = reservation["SelectedFlight"]?["DepartureTime"]?.ToString();
            var arrivalTime = reservation["SelectedFlight"]?["ArrivalTime"]?.ToString();
            var flightTime = reservation["SelectedFlight"]?["FlightTime"]?.ToString();
            var totalPrice = reservation["SelectedFlight"]?["TotalPrice"]?.ToString();
            var passengerName = reservation["PassengerName"]?.ToString();
            var passengerSurname = reservation["PassengerSurname"]?.ToString();
            var selectedSeat = reservation["SelectedSeat"]?.ToString();
            
            CLIService.DisplayReservationInfo(passengerName, passengerSurname, flightNumber, airline, aircraftType, departureTime, arrivalTime, flightTime, selectedSeat, totalPrice);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting reservation info: {ex.Message}");
        }
    }
}