namespace ItemPlanner;

using System.IO;

public class ItemListManager
{
    public List<PackingItem>? itemListData;
    PackingListSaver? fileSaver;

    public bool FileNameChecker(string name)
    {
        return File.Exists(name);
    }

    public string GenerateUniqueFileName()
    {
        while (true)
        {
            string uniqueName = $"{Guid.NewGuid()}.txt";
            if (!FileNameChecker(uniqueName))
            {
                return uniqueName;
            }
        }
    }

    public void ReadFile(string fileName)
    {
        fileSaver = new PackingListSaver(fileName);
        if (File.Exists($"{fileName}"))
        {
            itemListData = new List<PackingItem>();
            var listFileContent = File.ReadAllLines($"{fileName}");
            foreach (var line in listFileContent)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var splitted = line.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    string itemName = splitted[0];
                    int itemsPacked = Int32.Parse(splitted[1]);
                    int itemsToPack = Int32.Parse(splitted[2]);
                    Enum.TryParse(splitted[3], out ItemCategory category);
                    itemListData.Add(new PackingItem(itemName, itemsPacked, itemsToPack, category));
                }
            }
        }
    }

    public void ReWriteFile(string fileName)
    {
        File.Create(fileName).Close();
        if (itemListData != null)
        {
            foreach (var item in itemListData)
            {
                if (this.fileSaver != null)
                {
                    this.fileSaver.AppendData(item);
                }
            }

        }
    }
}