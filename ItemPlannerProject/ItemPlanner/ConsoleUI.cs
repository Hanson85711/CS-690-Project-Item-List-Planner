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
        AnsiConsole.MarkupLine($"You selected: [yellow]Create a Trip[/]");
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

