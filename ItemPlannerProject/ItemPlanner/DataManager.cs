namespace ItemPlanner;

public class DataManager
{
    TripFileSaver fileSaver;
    public List<TripData> TripData { get; }
    public string tripFileName;
    public DataManager() : this("trip-data.txt") { }

    public DataManager(string fileName)
    {
        fileSaver = new TripFileSaver(fileName);
        TripData = new List<TripData>();
        tripFileName = fileName;


        if (File.Exists(fileName))
        {
            var tripFileContent = File.ReadAllLines(fileName);
            foreach (var line in tripFileContent)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var splitted = line.Split("::", StringSplitOptions.RemoveEmptyEntries);
                    var tripDate = DateTime.Parse(splitted[0]);

                    var destinationName = splitted[1];

                    var tripType = splitted[2];
                    var tripPackList = splitted[3];

                    TripData.Add(new TripData(tripDate, destinationName, tripType, tripPackList));
                }
            }
        }
    }

    public void AddNewTripData(TripData data)
    {
        this.TripData.Add(data);
        this.fileSaver.AppendData(data);
    }
}