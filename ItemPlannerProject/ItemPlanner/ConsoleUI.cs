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
            ShowSavedTripMenu(savedTripChoice);
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

        var tripActionChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("Edit List", "[red]Back[/]"));
        if (tripActionChoice == "[red]Back[/]")
        {
            ShowManageTripsMenu();
        }
        else if (tripActionChoice == "Edit List")
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
                ChangeItemQuantity(tripChoice);
                break;
            case "Add Item":
                AddItem(tripChoice);
                break;
            case "Delete Item":
                ShowDeleteItem(tripChoice);
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

    //Function for editing Item Quantity
    private void ChangeItemQuantity(TripData tripData)
    {
        var grouped = returnGroupedItemsByCategory(tripData);

        //Selection Prompt
        var categoryToChange = new SelectionPrompt<object>()
            .Title("[green]Select Category To Edit[/]");

        var itemToChange = new SelectionPrompt<object>()
            .Title("[green]Select item to modify it's quantity[/]")
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
        categoryToChange.AddChoice("[red]Back[/]");

        //If Packing List is empty, force returns and ends function
        if (!grouped.Any())
        {
            AnsiConsole.MarkupLine("[red]No items available to edit.[/]");
            ShowSavedTripMenu(tripData);
            return;
        }

        //Prompts user to select a category
        var categorySelected = AnsiConsole.Prompt(categoryToChange);
        // If user selected the back option
        if (categorySelected is string)
        {
            EditPackingList(tripData);
            return;
        }
        //Grabs Group based on Category Chosen
        var itemGroup = grouped.FirstOrDefault(g => g.Key == (ItemCategory)categorySelected);

        //Adds Items From Selected Category To Choices
        if (itemGroup != null)
        {
            foreach (var item in itemGroup)
            {
                itemToChange.AddChoice(item);
            }

        }

        // Prompt user to select item
        PackingItem selected = (PackingItem)AnsiConsole.Prompt(itemToChange);

        // Prompt which quantity type to edit
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices(
                    "Change Quantity of Items Packed",
                    "Change Total Items Required",
                    "[red]Return To Packing List[/]"
                ));

        if (action == "[red]Return To Packing List[/]")
        {
            ShowSavedTripMenu(tripData);
            return;
        }

        // Shared prompt
        var quantityPrompt = new TextPrompt<int>("Enter the new quantity:");

        // Configure behavior based on selection
        string message;
        Action<int> updateAction;

        if (action == "Change Quantity of Items Packed")
        {
            message = $"Currently Modifying # of Packed: {selected.Name} ({selected.QuantityPacked}/{selected.QuantityToPack})";
            updateAction = q => selected.QuantityPacked = q;
        }
        else // Change Total Items Required
        {
            message = $"Currently Modifying # Required of: {selected.Name} ({selected.QuantityPacked}/{selected.QuantityToPack})";
            updateAction = q => selected.QuantityToPack = q;

            //Changes quantityPrompt to have input validation for this choice
            quantityPrompt = new TextPrompt<int>("Enter the new total required:")
            .Validate(q => q > 0
            ? ValidationResult.Success()
            : ValidationResult.Error("[red]Value must be greater than 0[/]"));
        }

        //Actions gets executed here
        AnsiConsole.MarkupLine(message);
        var newQuantity = AnsiConsole.Prompt(quantityPrompt);

        updateAction(newQuantity);

        itemListManager.ReWriteFile(tripData.ItemListName);
        ShowSavedTripMenu(tripData);
    }

    //Function for showing adding item menu logic to packinglist
    private void AddItem(TripData tripData)
    {
        string itemName = AnsiConsole.Ask<string>("What's the [green]name[/] of your item?");

        var categoryValues = Enum.GetValues<ItemCategory>();

        var categoryChoices = new SelectionPrompt<ItemCategory>()
        .Title("Select the [green]Category[/] for this item");

        foreach (var category in categoryValues)
        {
            categoryChoices.AddChoice(category);
            AnsiConsole.WriteLine("");
        }

        ItemCategory categorySelected = AnsiConsole.Prompt(categoryChoices);

        int itemsToPack = AnsiConsole.Prompt(new TextPrompt<int>("Enter quantity to pack:")
        .Validate(quantityToAdd =>
        {
            return quantityToAdd > 0
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Value must be greater than 0[/]");
        }));

        if (itemListManager.itemListData != null)
        {
            itemListManager.itemListData.Add(new PackingItem(itemName, 0, itemsToPack, categorySelected));
        }

        itemListManager.ReWriteFile(tripData.ItemListName);

        ShowSavedTripMenu(tripData);
    }

    private void ShowDeleteItem(TripData tripData)
    {
        var grouped = returnGroupedItemsByCategory(tripData);

        //Selection Prompt
        var categoryToChange = new SelectionPrompt<object>()
            .Title("[green]Select a Category[/]");

        var itemToChange = new SelectionPrompt<object>()
            .Title("[green]Select item to delete[/]")
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
        categoryToChange.AddChoice("[red]Back[/]");

        //If Packing List is empty, force returns and ends function
        if (!grouped.Any())
        {
            AnsiConsole.MarkupLine("[red]No items available to edit.[/]");
            ShowSavedTripMenu(tripData);
            return;
        }

        //Prompts user to select a category
        var categorySelected = AnsiConsole.Prompt(categoryToChange);
        // If user selected the back option
        if (categorySelected is string)
        {
            EditPackingList(tripData);
            return;
        }
        //Grabs Group based on Category Chosen
        var itemGroup = grouped.FirstOrDefault(g => g.Key == (ItemCategory)categorySelected);

        //Adds Items From Selected Category To Choices
        if (itemGroup != null)
        {
            foreach (var item in itemGroup)
            {
                itemToChange.AddChoice(item);
            }

        }

        // Prompt user to select item
        PackingItem selected = (PackingItem)AnsiConsole.Prompt(itemToChange);

        if (itemListManager.itemListData != null)
        {
            int itemIndex = itemListManager.itemListData.IndexOf(selected);

            itemListManager.itemListData.Remove(selected);
            itemListManager.ReWriteFile(tripData.ItemListName);
        }

        ShowSavedTripMenu(tripData);
    }
}