using FlightBooker.Models;
using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace FlightBooker.Services;

public class CLIService
{
    public static void DisplayWelcome()
    {
        // this ascii art made by me so, when you use credit to me thanks.
        string welcomeArt = @"
                              |
                              |
                              |
                          .-""""""""""-.
                 '------.'_________'.------'
                       '_/_/__|__\_\_' 
                      '               '
\_____________________'      ( )      '_____________________/
 `----|---|-/     \----'             '----/     \-|---|----'
            \     /     '._________.'     \     /
             `---'                         `---'   
        ";

        var welcomeMsg = new Markup("Welcome to [teal]FlightBooker![/] :waving_hand:");
        var descriptionMsg = new Markup("Search tickets, make reservation, check flight status.");
        var paddedWelcomeMsg = new Padder(welcomeMsg).PadLeft(18).PadTop(0).PadBottom(0);
        var paddedDescMsg = new Padder(descriptionMsg).PadLeft(4).PadTop(0);

        Console.WriteLine(welcomeArt);
        AnsiConsole.Write(paddedWelcomeMsg);
        AnsiConsole.Write(paddedDescMsg);
    }

    public static void DisplayMainMenu()
    {
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .MoreChoicesText("[grey]Move up and down to see more selections[/]")
                .AddChoices(new[]
                {
                    "Make Reservation",
                    "Show All Flights",
                    "Check Reservation",
                    "Cancel Reservation",
                })
        );

        switch (selectedOption)
        {
            case "Make Reservation":
                ReservationService.CreateReservation();
                break;
            case "Show All Flights":
                Console.WriteLine("Show All Flights");
                break;
            case "Check Reservation":
                Console.Write("PNR Code: ");
                var pnrCodeForInfo = Console.ReadLine();
                
                ReservationService.ReservationInformation(pnrCodeForInfo);
                GoBack();
                break;
            case "Cancel Reservation":
                Console.Write("PNR Code: ");
                var pnrCodeForCancel = Console.ReadLine();
                
                ReservationService.CancelReservation(pnrCodeForCancel);
                GoBack();
                break;
        }
    }

    public static void DisplayHeader(string title, string emoji)
    {
        string displayTitle =
            $"\n\u2500 {title} :{emoji}: \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500";
        var renderedTitle = Emoji.Replace(displayTitle);
        Console.WriteLine(renderedTitle);
    }

    public static void DisplayFlightsTable(JArray flights, string departureCity, string arrivalCity)
    {
        var table = new Table()
            .Title($"From: {departureCity} To: {arrivalCity}")
            .Border(TableBorder.Horizontal)
            .BorderColor(Color.Grey11)
            .AddColumn("[slateblue3_1]Flight Number[/]")
            .AddColumn("[slateblue3_1]Airline[/]")
            .AddColumn("[slateblue3_1]Aircraft Type[/]")
            .AddColumn("[slateblue3_1]Departure Date[/]")
            .AddColumn("[slateblue3_1]Arrival Date[/]")
            .AddColumn("[slateblue3_1]Flight Time[/]")
            .AddColumn("[slateblue3_1]Total Price[/]")
            .AddColumn("[slateblue3_1]Available Seats Count[/]");

        foreach (var flight in flights)
        {
            table.AddRow(
                flight["FlightNumber"]?.ToString(),
                flight["Airline"]?.ToString(),
                flight["Aircraft"]?["AircraftType"]?.ToString(),
                flight["DepartureTime"]?.ToString(),
                flight["ArrivalTime"]?.ToString(),
                flight["FlightTime"]?.ToString(),
                $"{flight["TotalPrice"]?.Value<decimal>():C}",
                (flight["AvailableSeats"]?.ToObject<List<string>>()?.Count ?? 0).ToString()
            );
        }

        AnsiConsole.Render(table);
    }

    public static string DisplaySeats(int rows, List<string> takenSeats)
    {
        int cols = 6;
        bool isSelected = false;
        string selectedSeat;

        bool[,] seatMap = new bool[rows, cols];
        
        foreach (var takenSeat in takenSeats)
        {
            char seatLetter = takenSeat[0];
            string seatNumberString = takenSeat.Substring(1);

            if (int.TryParse(seatNumberString, out int seatNumber) && seatNumber >= 1 && seatNumber <= rows)
            {
                int row = seatNumber - 1;
                int col = seatLetter - 'A';

                if (col >= 0 && col < cols)
                {
                    seatMap[row, col] = true;
                }
            }
        }

        Console.WriteLine("    A  B  C    D  E  F");
        
        for (int i = 0; i < rows; i++)
        {
            if (i >= 9)
            {
                Console.Write(i + 1 + " ");
            }
            else 
            {
                Console.Write(i + 1 + "  ");
            }

            for (int j = 0; j < cols; j++)
            {
                if (seatMap[i, j])
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[X]");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[_]");
                }

                if (j == 2)
                {
                    Console.Write("  ");
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }
        
        do
        {
            Console.Write("\nEnter seat selection (i.e. A5, C23): ");
            selectedSeat = Console.ReadLine().Trim().ToUpper();

            if (char.IsLetter(selectedSeat[0]) && char.IsDigit(selectedSeat[1]))
            {
                int selectedRowNum = int.Parse(selectedSeat.Substring(1));
                if (selectedRowNum >= 1 && selectedRowNum <= rows)
                {
                    int selectedCol = selectedSeat[0] - 'A';
                    if (!seatMap[selectedRowNum - 1, selectedCol])
                    {
                        isSelected = true;
                    }
                    else
                    {
                        Console.WriteLine("Sorry, the seat is already taken. Please choose another seat.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid seat number. Please enter a number between 1 and " + rows);
                }
            }
            else
            {
                Console.WriteLine("Invalid format. Please enter a valid seat selection.");
            }
        } while (!isSelected);
        
        return selectedSeat.ToUpper();
    }

    public static bool ShowReservationSummary(Flight targetFlight, string selectedSeat, string passengerName, string passengerSurname)
    {
        var table = new Table()
            .Title($"Dear, {passengerName} {passengerSurname}\n From: {targetFlight.Departure.City} To:  {targetFlight.Destination.City}")
            .Border(TableBorder.Horizontal)
            .BorderColor(Color.Grey11)
            .AddColumn("[slateblue3_1]Flight Number[/]")
            .AddColumn("[slateblue3_1]Airline[/]")
            .AddColumn("[slateblue3_1]Aircraft Type[/]")
            .AddColumn("[slateblue3_1]Departure Date[/]")
            .AddColumn("[slateblue3_1]Arrival Date[/]")
            .AddColumn("[slateblue3_1]Flight Time[/]")
            .AddColumn("[slateblue3_1]Selected Seat[/]")
            .AddColumn("[slateblue3_1]Total Price[/]");
        
        table.AddRow(
            targetFlight.FlightNumber,
            targetFlight.Airline,
            targetFlight.Aircraft.AircraftType,
            targetFlight.DepartureTime,
            targetFlight.ArrivalTime,
            targetFlight.FlightTime,
            selectedSeat,
            targetFlight.TotalPrice.ToString()
        );
        
        AnsiConsole.Render(table);
        
        Console.Write("Do you want to confirm this reservation? (y/N): ");
        var confirmation = Console.ReadLine();

        if (confirmation.ToLower() == "y")
        {
            var newReservation = new Reservation
            {
                Id = DataService.CreateReservationId(),
                SelectedFlight = targetFlight,
                PassengerName = passengerName,
                PassengerSurname = passengerSurname,
                SelectedSeat = selectedSeat,
                PNRCode = DataService.GeneratePNR()
            };
            
            var reservations = DataService.ReadJson(1);
            reservations.Add(JObject.FromObject(newReservation));
            
            DataService.SaveJson(1, reservations);
            Console.WriteLine("Reservation added successfully! Please keep your PNR code for future.");
            AnsiConsole.MarkupLine($"[slateblue3_1]PNR:[/] {newReservation.PNRCode}");
            return true;
        }

        GoBack();
        return false;
    }

    public static void DisplayReservationInfo(string passengerName, string passengerSurname, string flightNumber, string airline, string aircraftType, string departureTime, string arrivalTime, string flightTime, string selectedSeat, string totalPrice)
    {
        var table = new Table()
            .Title($"Dear, {passengerName} {passengerSurname}")
            .Border(TableBorder.Horizontal)
            .BorderColor(Color.Grey11)
            .AddColumn("[slateblue3_1]Flight Number[/]")
            .AddColumn("[slateblue3_1]Airline[/]")
            .AddColumn("[slateblue3_1]Aircraft Type[/]")
            .AddColumn("[slateblue3_1]Departure Date[/]")
            .AddColumn("[slateblue3_1]Arrival Date[/]")
            .AddColumn("[slateblue3_1]Flight Time[/]")
            .AddColumn("[slateblue3_1]Selected Seat[/]")
            .AddColumn("[slateblue3_1]Total Price[/]");

        table.AddRow(
            flightNumber,
            airline,
            aircraftType,
            departureTime,
            arrivalTime,
            flightTime,
            selectedSeat,
            totalPrice
        );

        AnsiConsole.Render(table);
        GoBack();
    }

    public static void GoBack()
    {
        Console.Write("Do you want to go back main menu? (y/n): ");
        var response = Console.ReadLine();
        
        while (string.IsNullOrWhiteSpace(response.ToLower()))
        {
            Console.WriteLine("Invalid response.");
            Console.Write("Do you want to go back main menu? (y/n): ");
            response = Console.ReadLine();
        }

        if (response == "y")
        {
            Console.Clear();
            DisplayWelcome();
            DisplayMainMenu();
        }
    }
}