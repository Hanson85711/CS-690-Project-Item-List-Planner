namespace ItemPlanner;

using System.Data;
using System.Reflection.Metadata;
using Spectre.Console;


public class InventoryUI
{
    private InventoryManager inventoryManager;
    private ConsoleUI theUI;

    public InventoryUI(InventoryManager inventory, ConsoleUI mainUI)
    {
        this.inventoryManager = inventory;
        this.theUI = mainUI;
    }


    private IEnumerable<IGrouping<ItemCategory, InventoryItem>> returnGroupedItemsByCategory()
    {
        IEnumerable<IGrouping<ItemCategory, InventoryItem>> grouped;
        if (!string.IsNullOrEmpty(inventoryManager.InventoryFileName))
        {
            if (inventoryManager.inventoryList != null)
            {
                grouped = inventoryManager.inventoryList.GroupBy(i => i.Category);
                return grouped;
            }
        }
        return Enumerable.Empty<IGrouping<ItemCategory, InventoryItem>>();
    }

    private void ListInventoryItemsInCategory(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            var status = item.IsOutOfStock ? "[green]In Stock[/]" : "[red]Out of stock[/]";
            AnsiConsole.MarkupLine($"- {item.Name} ({item.Quantity}) {status}");
        }
    }

    private void ShowInventoryList()
    {
        var grouped = returnGroupedItemsByCategory();
        foreach (var group in grouped)
        {
            AnsiConsole.WriteLine($"== {group.Key} ==");
            ListInventoryItemsInCategory(group);
            AnsiConsole.WriteLine("");
        }
    }


    public void ShowInventoryMenu()
    {
        //Panel For Displaying Trip Info
        var content = new Markup(
            "[grey]What do you want to do?[/]\n" +
            "[grey]──────────────────────────────────────[/]\n"
            );
        var titlePanel = new Panel(content)
            .DoubleBorder()
            .Header("Home Inventory", Justify.Center);

        AnsiConsole.Write(titlePanel);

        ShowInventoryList();

        var invenChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices("Add Item", "Edit Item Quantity", "Delete Item", "[red]Back[/]"));
        switch (invenChoice)
        {
            case "Add Item":
                AddItem();
                break;
            case "Edit Item Quantity":
                ChangeItemQuantity();
                break;
            case "Delete Item":
                ShowDeleteItem();
                break;
            case "[red]Back[/]":
                theUI.ShowMainMenu();
                break;
        }
    }


    private void AddItem()
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

        int itemStock = AnsiConsole.Prompt(new TextPrompt<int>("Enter quantity you currently have in stock:")
        .Validate(quantityToAdd =>
        {
            return quantityToAdd >= 0
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Value must be greater or equal to 0[/]");
        }));

        InventoryItem newItem = new InventoryItem(itemName, itemStock, categorySelected);
        inventoryManager.AddNewItem(newItem);

        ShowInventoryMenu();
    }


    //Function for editing Item Quantity
    private void ChangeItemQuantity()
    {
        var grouped = returnGroupedItemsByCategory();

        //Selection Prompt
        var categoryToChange = new SelectionPrompt<object>()
            .Title("[green]Select Category To Edit[/]");

        var itemToChange = new SelectionPrompt<object>()
            .Title("[green]Select item to modify it's quantity[/]")
            .UseConverter(item =>
            {
                var i = (InventoryItem)item;

                return $"- {i.Name} ({i.Quantity}) ";
            });


        //Adds categories to list
        foreach (var group in grouped)
        {
            categoryToChange.AddChoice(group.Key);
            AnsiConsole.WriteLine("");
        }
        categoryToChange.AddChoice("[red]Back[/]");

        //If Inventory List is empty, force returns and ends function
        if (!grouped.Any())
        {
            AnsiConsole.MarkupLine("[red]No items available to edit. Add some items first.[/]");
            ShowInventoryMenu();
            return;
        }

        //Prompts user to select a category
        var categorySelected = AnsiConsole.Prompt(categoryToChange);
        // If user selected the back option
        if (categorySelected is string)
        {
            ShowInventoryMenu();
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
        InventoryItem selected = (InventoryItem)AnsiConsole.Prompt(itemToChange);

        // Prompt which quantity type to edit
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices(
                    "Change Quantity of Item in stock",
                    "[red]Return To Inventory List[/]"
                ));

        if (action == "[red]Return To Inventory List[/]")
        {
            ShowInventoryMenu();
            return;
        }

        // Shared prompt
        var quantityPrompt = new TextPrompt<int>("Enter the new quantity:");

        //Sets action to take after selection prompt
        string message;
        Action<int> updateAction;

        message = $"Currently Modifying # of: {selected.Name}: ({selected.Quantity}) in stock.";
        updateAction = q => selected.Quantity = q;

        //Changes quantityPrompt to have input validation for this choice
        quantityPrompt = new TextPrompt<int>("Enter the new total required:")
        .Validate(q => q >= 0
        ? ValidationResult.Success()
        : ValidationResult.Error("[red]Value must be greater or equal to 0[/]"));

        //Actions gets executed here
        AnsiConsole.MarkupLine(message);
        var newQuantity = AnsiConsole.Prompt(quantityPrompt);

        updateAction(newQuantity);

        inventoryManager.ReWriteFile();
        ShowInventoryMenu();
    }


    private void ShowDeleteItem()
    {
        var grouped = returnGroupedItemsByCategory();

        //Selection Prompt
        var categoryToChange = new SelectionPrompt<object>()
            .Title("[green]Select a Category[/]");

        var itemToChange = new SelectionPrompt<object>()
            .Title("[green]Select item to delete[/]")
            .UseConverter(item =>
            {
                var i = (InventoryItem)item;

                return $"- {i.Name} ({i.Quantity})";
            });


        //Adds categories to list
        foreach (var group in grouped)
        {
            categoryToChange.AddChoice(group.Key);
            AnsiConsole.WriteLine("");
        }

        categoryToChange.AddChoice("[red]Back[/]");

        //If Inventory List is empty, force returns and ends function
        if (!grouped.Any())
        {
            AnsiConsole.MarkupLine("[red]No items available to edit. Add some items first.[/]");
            ShowInventoryMenu();
            return;
        }

        //Prompts user to select a category
        var categorySelected = AnsiConsole.Prompt(categoryToChange);
        // If user selected the back option
        if (categorySelected is string)
        {
            ShowInventoryMenu();
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
        InventoryItem selected = (InventoryItem)AnsiConsole.Prompt(itemToChange);

        if (inventoryManager.inventoryList != null)
        {
            int itemIndex = inventoryManager.inventoryList.IndexOf(selected);

            inventoryManager.inventoryList.Remove(selected);
            inventoryManager.ReWriteFile();
        }

        ShowInventoryMenu();
    }
}