namespace ItemPlanner;

class Program
{
    static void Main(string[] args)
    {
        var dataManager = new DataManager();
        var itemListManager = new ItemListManager();

        var consoleUI = new ConsoleUI(dataManager, itemListManager);
        var packingUI = new PackingListUI(itemListManager, consoleUI);
        
        consoleUI.SetPackingUI(packingUI);
        consoleUI.Show();
    }
}