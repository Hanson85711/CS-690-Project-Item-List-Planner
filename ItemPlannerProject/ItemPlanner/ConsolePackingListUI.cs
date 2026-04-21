namespace ItemPlanner;

using System.Data;
using System.Reflection.Metadata;
using Spectre.Console;


public class PackingListUI
{
    private ItemListManager itemListManager;
    private ConsoleUI theUI;

    public PackingListUI(ItemListManager itemListManager, ConsoleUI theUI)
    {
        this.itemListManager = itemListManager;
        this.theUI = theUI;
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

    private void ListItemsToBuy(IEnumerable<PackingItem> items)
    {
        foreach (var item in items)
        {
            if (!item.IsFullyPacked)
            {
                AnsiConsole.MarkupLine($"- {item.Name} ({item.QuantityToPack - item.QuantityPacked})");
            }
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

    private void ShowTripShoppingList(TripData currentTrip)
    {
        var grouped = returnGroupedItemsByCategory(currentTrip);

        //Panel For Displaying Trip Info
        var content = new Markup(
            "[grey]Here are the items you're still missing for: [/]\n" +
            "[grey]──────────────────────────────────────[/]\n" +
            $"[yellow]{currentTrip.TripType}[/] at [blue]{currentTrip.Destination}[/] on [green]{currentTrip.TripDate:yyyy-MM-dd}[/]\n"
            );
        var titlePanel = new Panel(content)
            .DoubleBorder()
            .Header("Shopping List", Justify.Center);

        AnsiConsole.Write(titlePanel);

        foreach (var group in grouped)
        {
            //Determines if prints the category or not based on if group has Unpacked items
            var itemExists = group.FirstOrDefault(item => item.IsFullyPacked == false);
            if (itemExists != null)
            {
                AnsiConsole.WriteLine($"== {group.Key} ==");
            }
            ListItemsToBuy(group);
            AnsiConsole.WriteLine("");
        }

        var returnOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("[red]Back[/]"));

        if (returnOption == "[red]Back[/]")
        {
            ShowSavedTripMenu(currentTrip);
        }
    }

    public void ShowSavedTripMenu(TripData tripChoice)
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
            .AddChoices("Edit List", "Create Shopping List", "[red]Back[/]"));
        if (tripActionChoice == "[red]Back[/]")
        {
            theUI.ShowManageTripsMenu();
        }
        else if (tripActionChoice == "Edit List")
        {
            EditPackingList(tripChoice);
        }
        else if (tripActionChoice == "Create Shopping List")
        {
            ShowTripShoppingList(tripChoice);
        }
    }

    private void CreateShoppingList(TripData tripData)
    {
        ShowTripShoppingList(tripData);
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