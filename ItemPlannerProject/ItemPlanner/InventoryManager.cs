namespace ItemPlanner;

using System.IO;

public class InventoryManager
{
    InventoryListSaver inventoryFileSaver;
    public List<InventoryItem> inventoryList { get; }
    public string InventoryFileName;

    public InventoryManager() : this("inventory-data.txt") { }

    public InventoryManager(string fileName)
    {
        inventoryFileSaver = new InventoryListSaver(fileName);
        inventoryList = new List<InventoryItem>();
        InventoryFileName = fileName;


        if (File.Exists(fileName))
        {
            var inventoryFileContent = File.ReadAllLines(fileName);
            foreach (var line in inventoryFileContent)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var splitted = line.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    string itemName = splitted[0];
                    int itemQuantity = Int32.Parse(splitted[1]);
                    Enum.TryParse(splitted[2], out ItemCategory category);
                    inventoryList.Add(new InventoryItem(itemName, itemQuantity, category));
                }
            }
        }
    }

    public void AddNewItem(InventoryItem item)
    {
        this.inventoryList.Add(item);
        this.inventoryFileSaver.AppendData(item);
    }

    public void ReWriteFile()
    {
        File.Create(InventoryFileName).Close();
        if (inventoryList != null)
        {
            foreach (var item in inventoryList)
            {
                this.inventoryFileSaver.AppendData(item);
            }
        }
    }
}