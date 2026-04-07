namespace ItemPlanner;

public class TripData {
    public DateTime TripDate { get; }
    public string Destination { get; }
    public string TripType { get; }

    public TripData(DateTime tripDate, string destination, string tripType) {
        this.TripDate = tripDate;
        this.Destination = destination;
        this.TripType = tripType;
    }

    public override string ToString() {
        return $"{this.TripType} at {this.Destination} on {this.TripDate:yyyy-MM-dd}";
    }
}