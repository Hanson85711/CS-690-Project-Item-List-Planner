namespace ItemPlanner;

using System.Data;
using System.Reflection.Metadata;
using Spectre.Console;

public class ConsoleUI
{
    private DataManager dataManager;
    private ItemListManager itemListManager;
    private PackingListUI? packingUI;

    public ConsoleUI(DataManager dataManager, ItemListManager itemListManager)
    {
        this.dataManager = dataManager;
        this.itemListManager = itemListManager;
    }

    public void SetPackingUI(PackingListUI packingUI)
    {
        this.packingUI = packingUI;
    }
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
                .AddChoices("Manage Trips", "[red]Exit[/]"));

        AnsiConsole.MarkupLine($"You selected: [yellow]{mainChoice}[/]");

        if (mainChoice == "Manage Trips")
        {
            ShowManageTripsMenu();
        }
    }

    public void ShowManageTripsMenu()
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
                .AddChoices("Create a Trip", "View Saved Trips", "Delete a Trip", "[red]Return to Main Menu[/]"));

        switch (tripManageChoice)
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
            case "[red]Return to Main Menu[/]":
                ShowMainMenu();
                break;
        }
    }

    private void ShowCreateTrip()
    {
        //Function for prompting user to input required data to create a TripData
        var destination = AnsiConsole.Ask<string>("Enter your [green]destination[/]");
        //Validates if User Input is a proper date
        var tripDate = AnsiConsole.Prompt
        (
            new TextPrompt<DateTime>("Enter a [green]date[/] (e.g. 2026 04 26):")
                .Validate(d =>
                    d < DateTime.Today
                        ? ValidationResult.Error("[red]Date must be today or later[/]")
                        : ValidationResult.Success())
        );

        AnsiConsole.MarkupLine("[bold yellow]What kind of trip is this?[/]");
        string packingListName;

        //Uses the preset packing list based on trip type or generates a brand new one
        var tripTypeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("Beach Trip", "Sightseeing Trip", "Overseas Business Trip", "Enter your own"));

        if (tripTypeChoice == "Enter your own")
        {
            tripTypeChoice = AnsiConsole.Ask<string>("Enter your [green]trip type[/]");

            packingListName = itemListManager.GenerateUniqueFileName();
            File.Create(Path.Combine(itemListManager.absFilePath, packingListName)).Close();
        }
        else
        {
            AnsiConsole.MarkupLine("[bold]Would you like to use the preset packing list?[/]");
            var presetChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices("Yes", "No"));
            //Creates Clone Packing List .txt file based on preset
            if (presetChoice == "Yes")
            {
                string presetName = "presetlist1.txt";
                packingListName = itemListManager.GenerateUniqueFileName();
                File.Copy(presetName, Path.Combine(itemListManager.absFilePath, packingListName), true);
            }
            //Creates Empty Packing List .txt file
            else
            {
                packingListName = itemListManager.GenerateUniqueFileName();
                File.Create(Path.Combine(itemListManager.absFilePath, packingListName)).Close();
            }
        }
        //Creates the TripData using the user input data and adds it to the dataManager
        TripData data = new TripData(tripDate, destination, tripTypeChoice, packingListName);
        dataManager.AddNewTripData(data);
        AnsiConsole.MarkupLine($"You entered: [yellow]{tripTypeChoice}[/] at [blue]{destination}[/] on [green]{tripDate:yyyy-MM-dd}[/]");
        ShowManageTripsMenu();
    }

    private TripOptions ShowListOfTrips()
    {
        var options = dataManager.TripData
            .Select(t => new TripOptions
            {
                Display = t.ToString(),
                Trip = t
            })
            .ToList();

        // Add Back option
        options.Add(new TripOptions
        {
            Display = "[red]Back[/]",
            Trip = null
        });

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<TripOptions>()
                .UseConverter(o => o.Display)
                .AddChoices(options)
        );

        return choice;
    }

    private void ShowViewSavedTrips()
    {
        TripData savedTripChoice;

        var choice = ShowListOfTrips();
        // Handle result
        if (choice.Trip == null)
        {
            // Back selected
            ShowManageTripsMenu();
        }
        else
        {
            savedTripChoice = choice.Trip;
            packingUI.ShowSavedTripMenu(savedTripChoice);
        }
    }

    private void ShowDeleteTrip()
    {
        TripData savedTripChoice;

        var choice = ShowListOfTrips();
        if (choice.Trip == null)
        {
            // Back selected
            ShowManageTripsMenu();
        }
        else
        {
            savedTripChoice = choice.Trip;
            itemListManager.DeletePackingList(savedTripChoice.ItemListName);
            int tripIndex = dataManager.TripData.IndexOf(savedTripChoice);

            dataManager.TripData.Remove(savedTripChoice);

            var lines = File.ReadAllLines(dataManager.tripFileName).ToList();
            if (lines.Count > tripIndex)
            {
                lines.RemoveAt(tripIndex);
                File.WriteAllLines(dataManager.tripFileName, lines);
            }

            ShowManageTripsMenu();
        }
    }
}