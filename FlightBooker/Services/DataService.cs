using FlightBooker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlightBooker.Services;

public class DataService
{
    public static int GetLastId(int selectedJson)
    {
        var json = ReadJson(selectedJson);
        var lastIdString = json.Last["Id"].ToString();
        var lastId = int.Parse(lastIdString);
        return lastId;
    }
    
    public static string GetFolderPath(string folderName)
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.Parent.FullName;
        string folderPath = Path.Combine(projectDirectory, folderName);

        return folderPath;
    }

    public static string GetFilePath(string fileName)
    {
        string folderPath = GetFolderPath("Data");
        string filePath = folderPath + fileName;

        return filePath;
    }

    public static JArray ReadJson(int selectedJson)
    {
        try
        {
            var selectedPath = (selectedJson == 0) ? GetFilePath("/Flights.json") : GetFilePath("/Reservations.json");
            var rawData = File.ReadAllText(selectedPath);
            var jsonData = JArray.Parse(rawData);
            return jsonData;
        }
        catch (JsonException exc)
        {
            Console.WriteLine($"Error reading and parsing JSON: {exc.Message}");
            return null;
        }
        catch (IOException exc)
        {
            Console.WriteLine($"Error reading file: {exc.Message}");
            return null;
        }
    }
    
    public static void SaveJson(int selectedJson, JArray jsonArray)
    {
        try
        {
            var selectedPath = (selectedJson == 0) ? GetFilePath("/Flights.json") : GetFilePath("/Reservations.json");
            File.WriteAllText(selectedPath, jsonArray.ToString());
        }
        catch (JsonException exc)
        {
            Console.WriteLine($"Error reading and parsing JSON: {exc.Message}");
        }
        catch (IOException exc)
        {
            Console.WriteLine($"Error reading file: {exc.Message}");
        }
    }
    
    public static void AddData(int selectedJson, JObject newData)
    {
        try
        {
            var jsonArray = ReadJson(selectedJson);

            if (jsonArray != null)
            {
                jsonArray.Add(newData);
                SaveJson(selectedJson, jsonArray);
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Error adding data: {exc.Message}");
        }
    }
    
    public static JArray FilterFlights(string departure, string arrival)
    {
        var flights = ReadJson(0);
    
        if (flights == null)
            return new JArray();

        var filteredFlights = new JArray();

        foreach (var flight in flights)
        {
            var departureCity = flight["Departure"]["City"].ToString();
            var arrivalCity = flight["Destination"]["City"].ToString();

            if (departureCity.Equals(departure, StringComparison.OrdinalIgnoreCase) &&
                arrivalCity.Equals(arrival, StringComparison.OrdinalIgnoreCase))
            {
                filteredFlights.Add(flight);
            }
        }

        return filteredFlights;
    }
    
    public static Flight GetFlightByFlightNumber(string flightNumber)
    {
        JArray flightsJson = ReadJson(0);

        if (flightsJson == null)
        {
            Console.WriteLine("Error reading flights data.");
            return null;
        }
        
        JObject targetFlightJson = flightsJson.FirstOrDefault(f => f["FlightNumber"].ToString().ToLower() == flightNumber.ToLower()) as JObject;

        if (targetFlightJson != null)
        {
            return targetFlightJson.ToObject<Flight>();
        }

        return null;
    }
    
    public static string GeneratePNR()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static int CreateReservationId()
    {
        var reservations = ReadJson(1);

        if (reservations == null)
        {
            Console.WriteLine("Error reading reservations data.");
            return -1;
        }
        
        return reservations.Count > 0 ? reservations.Max(r => r["Id"].Value<int>()) + 1 : 1;
    }
    
    public static void UpdateFlight(Flight targetFlight)
    {
        var flights = ReadJson(0);

        if (flights == null)
        {
            Console.WriteLine("Error reading flights data.");
            return;
        }

        var existingFlight = flights.FirstOrDefault(f => f["FlightNumber"].ToString().Equals(targetFlight.FlightNumber, StringComparison.OrdinalIgnoreCase));

        if (existingFlight != null)
        {
            existingFlight["AvailableSeats"] = JToken.FromObject(targetFlight.AvailableSeats);
            existingFlight["TakenSeats"] = JToken.FromObject(targetFlight.TakenSeats);
            SaveJson(0, flights);
        }
        else
        {
            Console.WriteLine($"Flight with '{targetFlight.FlightNumber}' flight number not found.");
        }
    }
}