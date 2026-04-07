namespace ItemPlanner;

using Spectre.Console;

public class ConsoleUI
{
    public void Show()
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        //Main Menu Display
        var titlePanel = new Panel("Main Menu")
            .DoubleBorder()
            .Padding(3, 0)
            .Header("Item Planner", Justify.Center);


        AnsiConsole.Write(titlePanel);
        AnsiConsole.WriteLine();
        var mainChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("Manage Trips", "Exit"));
        
        AnsiConsole.MarkupLine($"You selected: [yellow]{mainChoice}[/]");

        if (mainChoice == "Manage Trips")
        {
            ShowManageTripsMenu();
        }
    }

    private void ShowManageTripsMenu()
    {
        //Menu for when user selects manage trips

        var titlePanel = new Panel("Manage your trips")
            .DoubleBorder()
            .Padding(3, 0)
            .Header("Item Planner", Justify.Center);
            
        AnsiConsole.Write(titlePanel);
        AnsiConsole.WriteLine();

         var tripManageChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("Create a Trip", "View Saved Trips", "Delete a Trip", "Return to Main Menu")); 

        switch(tripManageChoice)
            {
                case "Create a Trip":
                    var createTripPanel = new Panel("Create a Trip")
                        .DoubleBorder()
                        .Padding(3, 0)
                        .Header("Item Planner", Justify.Center);
                    AnsiConsole.Write(createTripPanel);                    
                    ShowCreateTrip();
                    break;
                case "View Saved Trips":
                    ShowViewSavedTrips();
                    break;
                case "Delete a Trip":
                    ShowDeleteTrip();
                    break;
                case "Return to Main Menu":
                    ShowMainMenu();
                    break;
            };      
    }

    private void ShowCreateTrip()
    {
        var destination = AnsiConsole.Ask<string>("Enter your [green]destination[/]");
        var tripDate = AnsiConsole.Prompt
        (
            new TextPrompt<DateTime>("Enter a [green]date[/] (e.g. 2026 04 26):")
                .Validate(d =>
                    d < DateTime.Today
                        ? ValidationResult.Error("[red]Date must be today or later[/]")
                        : ValidationResult.Success())
        );

        AnsiConsole.MarkupLine("[bold yellow]What kind of trip is this?[/]");
        var tripTypeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("Beach Trip", "Sightseeing Trip", "Overseas Business Trip", "Enter your own"));

        if (tripTypeChoice == "Enter your own")
        {
            tripTypeChoice = AnsiConsole.Ask<string>("Enter your [green]trip type[/]");
        } 

        AnsiConsole.MarkupLine($"You entered: [yellow]{tripTypeChoice}[/] at [blue]{destination}[/] on [green]{tripDate:yyyy-MM-dd}[/]");
    }

    private void ShowViewSavedTrips()
    {
        AnsiConsole.MarkupLine($"You selected: [yellow]ViewSavedTrips[/]");
    }

    private void ShowDeleteTrip()
    {
        AnsiConsole.MarkupLine($"You selected: [yellow]Delete a Trip[/]");
    }
}

