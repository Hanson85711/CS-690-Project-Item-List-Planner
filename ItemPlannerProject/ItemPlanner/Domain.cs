namespace ItemPlanner;

public class TripData
{
    public DateTime TripDate { get; }
    public string Destination { get; }
    public string TripType { get; }
    public string ItemListName { get; private set; }

    public TripData(DateTime tripDate, string destination, string tripType, string itemListName)
    {
        this.TripDate = tripDate;
        this.Destination = destination;
        this.TripType = tripType;
        this.ItemListName = itemListName;
    }

    public override string ToString()
    {
        return $"{this.TripType} at {this.Destination} on {this.TripDate:yyyy-MM-dd}";
    }
}
public class Options
{
    public required string Display { get; set; }
}

public class TripOptions : Options
{
    public TripData? Trip { get; set; } // null means it's not a trip
}

public enum ItemCategory
{
    Clothing,
    Toiletries,
    Electronics,
    Documents,
    Misc
}
public class PackingItem
{
    public string Name { get; set; } = "";
    public int QuantityPacked { get; set; } = 0;
    public int QuantityToPack { get; set; }
    public ItemCategory Category { get; set; }

    // AutoCalculated bool
    public bool IsFullyPacked => QuantityPacked >= QuantityToPack;

    public PackingItem(string name, int quantityPacked, int quantityToPack, ItemCategory category)
    {
        this.Name = name;
        this.QuantityPacked = quantityPacked;
        this.QuantityToPack = quantityToPack;
        this.Category = category;
    }
}

public class InventoryItem
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 0;
    public ItemCategory Category { get; set; }

    // AutoCalculated bool
    public bool IsOutOfStock => Quantity > 0;

    public InventoryItem(string name, int quantity, ItemCategory category)
    {
        this.Name = name;
        this.Quantity = quantity;
        this.Category = category;
    }
}