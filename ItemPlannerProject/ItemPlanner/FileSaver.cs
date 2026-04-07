namespace ItemPlanner;
using System.IO;


public abstract class FileSaverBase
{
    protected string FileName;

    public FileSaverBase(string fileName)
    {
        FileName = fileName;

        if (!File.Exists(FileName))
        {
            File.Create(FileName).Close();
        }
    }

    public void AppendLine(string line)
    {
        File.AppendAllText(FileName, line + Environment.NewLine);
    }
}

public class TripFileSaver : FileSaverBase
{
    public TripFileSaver(string fileName) : base(fileName) { }

    public void AppendData(TripData data)
    {
        var line = $"{data.TripDate}::{data.Destination}::{data.TripType}::{data.ItemListName}";
        File.AppendAllText(FileName, line + Environment.NewLine);
    }
}

public class PackingListSaver : FileSaverBase
{
    public PackingListSaver(string fileName) : base(fileName) { }

    public void AppendData(PackingItem data)
    {
        var line = $"{data.Name}|{data.QuantityPacked}|{data.QuantityToPack}|{data.Category}";
        File.AppendAllText(FileName, line + Environment.NewLine);
    }
}
