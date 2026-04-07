namespace ItemPlanner;

public class DataManager
{
    FileSaver fileSaver;
    public List<TripData> TripData { get; }

    public DataManager()
    {
        fileSaver = new FileSaver("trip-data.txt");
        TripData = new List<TripData>();


        if(File.Exists("trip-data.txt")) {
            var tripFileContent = File.ReadAllLines("trip-data.txt");
            foreach(var line in tripFileContent) {
                if (!string.IsNullOrEmpty(line)) 
                {
                    var splitted = line.Split("::",StringSplitOptions.RemoveEmptyEntries);
                    var tripDate = DateTime.Parse(splitted[0]);

                    var destinationName= splitted[1];

                    var tripType = splitted[2];

                    TripData.Add(new TripData(tripDate,destinationName,tripType));
                }
            }
        }
    }

    public void AddNewTripData(TripData data) {
        this.TripData.Add(data);
        this.fileSaver.AppendData(data);
    }   
}