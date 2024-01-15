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
                .AddChoices(new[] {
                    "Most Popular Flights",
                    "Make Reservation",
                    "Cancel Reservation",
                    "Check Flight",
                })
        );

        switch (selectedOption)
        {
            case "Most Popular Flights":
                Console.WriteLine("Most Popular Flights");
                break;
            case "Make Reservation":
                Console.WriteLine("Make Reservation");
                break;
            case "Cancel Reservation":
                Console.WriteLine("Cancel Reservation");
                break;
            case "Check Flight":
                Console.WriteLine("Check Flight");
                break;
        }
    }
}