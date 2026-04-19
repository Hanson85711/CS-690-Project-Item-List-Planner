namespace ItemPlanner;

using System.Data;
using System.Reflection.Metadata;
using Spectre.Console;

public class ConsoleUI
{
    DataManager dataManager;
    ItemListManager itemListManager;
    public ConsoleUI()
    {
        dataManager = new DataManager();
        itemListManager = new ItemListManager();
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
            File.Create(packingListName).Close();
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

    private void ShowViewSavedTrips()
    {
        TripData savedTripChoice;

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

        // Handle result
        if (choice.Trip == null)
        {
            // Back selected
            ShowManageTripsMenu();
        }
        else
        {
            savedTripChoice = choice.Trip;
            ShowSavedTripMenu(savedTripChoice);
        }
    }

    private void ShowDeleteTrip()
    {
        AnsiConsole.MarkupLine($"You selected: [yellow]Delete a Trip[/]");
    }

    private IEnumerable<IGrouping<ItemCategory, PackingItem>> returnGroupedItemsByCategory(TripData tripChoice)
    {
        IEnumerable<IGrouping<ItemCategory, PackingItem>> grouped;
        if (!string.IsNullOrEmpty(tripChoice.ItemListName))
        {
            itemListManager.ReadFile(tripChoice.ItemListName);
            if (itemListManager.itemListData != null)
            {
                grouped = itemListManager.itemListData.GroupBy(i => i.Category);
                return grouped;
            }
        }

        return Enumerable.Empty<IGrouping<ItemCategory, PackingItem>>();
    }

    private void ListItemsInCategory(IEnumerable<PackingItem> items)
    {
        foreach (var item in items)
        {
            var status = item.IsFullyPacked ? "[green]✔ Packed[/]" : "[yellow]In Progress[/]";
            AnsiConsole.MarkupLine($"- {item.Name} ({item.QuantityPacked}/{item.QuantityToPack}) {status}");
        }
    }

    private void ShowTripPackingList(TripData currentTrip)
    {
        var grouped = returnGroupedItemsByCategory(currentTrip);
        foreach (var group in grouped)
        {
            AnsiConsole.WriteLine($"== {group.Key} ==");
            ListItemsInCategory(group);
            AnsiConsole.WriteLine("");
        }
    }

    private void ShowSavedTripMenu(TripData tripChoice)
    {
        //Panel For Displaying Trip Info
        var content = new Markup(
            "[grey]What do you want to do?[/]\n" +
            "[grey]──────────────────────────────────────[/]\n" +
            $"[yellow]{tripChoice.TripType}[/] at [blue]{tripChoice.Destination}[/] on [green]{tripChoice.TripDate:yyyy-MM-dd}[/]\n"
            );
        var titlePanel = new Panel(content)
            .DoubleBorder()
            .Header("Item Planner", Justify.Center);

        AnsiConsole.Write(titlePanel);

        ShowTripPackingList(tripChoice);

        var tripTypeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("Edit List", "[red]Back[/]"));
        if (tripTypeChoice == "[red]Back[/]")
        {
            ShowManageTripsMenu();
        }
        else if (tripTypeChoice == "Edit List")
        {
            EditPackingList(tripChoice);
        }
    }

    private void EditPackingList(TripData tripChoice)
    {
        itemListManager.ReadFile(tripChoice.ItemListName);
        var editChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("Mark Items as Packed", "Edit Item Quantity", "Add Item", "Delete Item", "[red]Back[/]"));
        switch (editChoice)
        {
            case "Mark Items as Packed":
                ChangeItemStatus(tripChoice);
                ShowSavedTripMenu(tripChoice);
                break;
            case "Edit Item Quantity":
                AnsiConsole.MarkupLine($"You selected: [yellow]Edit Item Quantity[/]");
                break;
            case "Add Item":
                AnsiConsole.MarkupLine($"You selected: [yellow]Add Item[/]");
                break;
            case "Delete Item":
                AnsiConsole.MarkupLine($"You selected: [yellow]Delete Item[/]");
                break;
            case "[red]Back[/]":
                ShowSavedTripMenu(tripChoice);
                break;
        }
    }

    //Function for Changing Item Packed Status
    private void ChangeItemStatus(TripData tripData)
    {
        var grouped = returnGroupedItemsByCategory(tripData);

        //Selection Prompt
        var categoryToChange = new SelectionPrompt<ItemCategory>()
            .Title("[green]Select Category To Edit[/]");

        var itemsToChange = new MultiSelectionPrompt<object>()
            .Title("[green]Select items to toggle packed status[/]")
            .NotRequired()
            .UseConverter(item =>
            {
                var i = (PackingItem)item;

                var status = i.IsFullyPacked
                    ? "[green]✔ Packed[/]"
                    : "[yellow]In Progress[/]";

                return $"- {i.Name} ({i.QuantityPacked}/{i.QuantityToPack}) {status}";
            });


        //Adds categories to list
        foreach (var group in grouped)
        {
            categoryToChange.AddChoice(group.Key);
            AnsiConsole.WriteLine("");
        }
        //If Packing List is empty, force returns and ends function
        if (!grouped.Any())
        {
            AnsiConsole.MarkupLine("[red]No items available.[/]");
            return;
        }
        //Prompts user to select a category
        var categorySelected = AnsiConsole.Prompt(categoryToChange);
        //Grabs Group based on Category Chosen
        var itemGroup = grouped.FirstOrDefault(g => g.Key == categorySelected);

        //Adds Items From Selected Category To Choices
        if (itemGroup != null)
        {
            foreach (var item in itemGroup)
            {
                itemsToChange.AddChoice(item);
            }

        }

        //Prompts user to select items from list
        var selected = AnsiConsole.Prompt(itemsToChange);

        foreach (var obj in selected)
        {
            if (obj is PackingItem item)
            {
                item.QuantityPacked = item.QuantityToPack;
            }
        }

        itemListManager.ReWriteFile(tripData.ItemListName);
    }
}