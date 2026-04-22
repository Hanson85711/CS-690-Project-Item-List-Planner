namespace ItemPlanner;

class Program
{
    static void Main(string[] args)
    {
        var dataManager = new DataManager();
        var itemListManager = new ItemListManager();
        var inventoryManager = new InventoryManager();

        var consoleUI = new ConsoleUI(dataManager, itemListManager);
        var packingUI = new PackingListUI(itemListManager, consoleUI);
        var inventoryUI = new InventoryUI(inventoryManager, consoleUI);
        
        consoleUI.SetPackingUI(packingUI);
        consoleUI.SetInventoryUI(inventoryUI);
        consoleUI.Show();
    }
}