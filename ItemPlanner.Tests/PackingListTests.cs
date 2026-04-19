using ItemPlanner;
using System.IO;
using Xunit;

namespace ItemPlanner.Tests;

public class DataManagerTests
{
    [Fact]
    public void Constructor_LoadsTripDataFromFile_WhenFileExists()
    {
        var tempFile = Path.GetTempFileName();
        var testData = "2026-04-20::Paris::Sightseeing::presetlist1.txt\n2026-05-01::Beach::Beach Trip::custom.txt";
        File.WriteAllText(tempFile, testData);

        var dataManager = new DataManager(tempFile);

        Assert.Equal(2, dataManager.TripData.Count);
        Assert.Equal("Paris", dataManager.TripData[0].Destination);
        Assert.Equal("Beach", dataManager.TripData[1].Destination);

        File.Delete(tempFile);
    }

    [Fact]
    public void Constructor_InitializesEmptyList_WhenFileDoesNotExist()
    {
        var nonExistentFile = "nonexistent.txt";

        var dataManager = new DataManager(nonExistentFile);

        Assert.Empty(dataManager.TripData);

        if (File.Exists(nonExistentFile))
        {
            File.Delete(nonExistentFile);
        }
    }

    [Fact]
    public void AddNewTripData_AddsTripAndAppendsToFile()
    {
        var tempFile = Path.GetTempFileName();
        var dataManager = new DataManager(tempFile);
        var tripData = new TripData(DateTime.Parse("2026-06-15"), "Tokyo", "Business", "business.txt");

        dataManager.AddNewTripData(tripData);

        Assert.Single(dataManager.TripData);
        Assert.Equal("Tokyo", dataManager.TripData[0].Destination);

        // Verify file content
        var fileContent = File.ReadAllText(tempFile);
        Assert.Contains("06/15/2026 00:00:00::Tokyo::Business::business.txt", fileContent);

        File.Delete(tempFile);
    }
}

public class TripDataTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var date = DateTime.Parse("2026-04-20");
        var destination = "Paris";
        var tripType = "Sightseeing";
        var itemListName = "presetlist1.txt";


        var tripData = new TripData(date, destination, tripType, itemListName);

        Assert.Equal(date, tripData.TripDate);
        Assert.Equal(destination, tripData.Destination);
        Assert.Equal(tripType, tripData.TripType);
        Assert.Equal(itemListName, tripData.ItemListName);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var date = DateTime.Parse("2026-04-20");
        var tripData = new TripData(date, "Paris", "Sightseeing", "presetlist1.txt");

        var result = tripData.ToString();

        Assert.Equal("Sightseeing at Paris on 2026-04-20", result);
    }
}

public class PackingItemTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {

        var item = new PackingItem("T-shirt", 2, 5, ItemCategory.Clothing);

        Assert.Equal("T-shirt", item.Name);
        Assert.Equal(2, item.QuantityPacked);
        Assert.Equal(5, item.QuantityToPack);
        Assert.Equal(ItemCategory.Clothing, item.Category);
    }

    [Theory]
    [InlineData(5, 5, true)]
    [InlineData(6, 5, true)]
    [InlineData(4, 5, false)]
    [InlineData(0, 5, false)]
    public void IsFullyPacked_ReturnsCorrectValue(int packed, int toPack, bool expected)
    {
        var item = new PackingItem("Test", packed, toPack, ItemCategory.Misc);


        Assert.Equal(expected, item.IsFullyPacked);
    }
}

public class ItemListManagerTests
{
    [Fact]
    public void FileNameChecker_ReturnsTrue_WhenFileExists()
    {
        var tempFile = Path.GetTempFileName();
        var manager = new ItemListManager();

        var result = manager.FileNameChecker(tempFile);

        Assert.True(result);

        File.Delete(tempFile);
    }

    [Fact]
    public void FileNameChecker_ReturnsFalse_WhenFileDoesNotExist()
    {
        var manager = new ItemListManager();
        var nonExistentFile = "nonexistent.txt";


        var result = manager.FileNameChecker(nonExistentFile);


        Assert.False(result);
    }

    [Fact]
    public void GenerateUniqueFileName_ReturnsUniqueName()
    {
        var manager = new ItemListManager();

        var name1 = manager.GenerateUniqueFileName();
        var name2 = manager.GenerateUniqueFileName();

        Assert.NotEqual(name1, name2);
        Assert.EndsWith(".txt", name1);
        Assert.EndsWith(".txt", name2);
    }

    [Fact]
    public void ReadFile_LoadsItemsCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        var testData = "T-shirt|2|5|Clothing\nToothbrush|1|1|Toiletries\nPassport|1|1|Documents";
        File.WriteAllText(tempFile, testData);
        var manager = new ItemListManager();

        manager.ReadFile(tempFile);

        Assert.NotNull(manager.itemListData);
        Assert.Equal(3, manager.itemListData.Count);
        Assert.Equal("T-shirt", manager.itemListData[0].Name);
        Assert.Equal(2, manager.itemListData[0].QuantityPacked);
        Assert.Equal(5, manager.itemListData[0].QuantityToPack);
        Assert.Equal(ItemCategory.Clothing, manager.itemListData[0].Category);

        Assert.Equal("Toothbrush", manager.itemListData[1].Name);
        Assert.Equal(ItemCategory.Toiletries, manager.itemListData[1].Category);

        Assert.Equal("Passport", manager.itemListData[2].Name);
        Assert.Equal(ItemCategory.Documents, manager.itemListData[2].Category);

        File.Delete(tempFile);
    }

    [Fact]
    public void ReadFile_SetsItemListDataToNull_WhenFileDoesNotExist()
    {
        var manager = new ItemListManager();
        var nonExistentFile = "nonexistent.txt";

        manager.ReadFile(nonExistentFile);

        Assert.Null(manager.itemListData);
    }
}

public class FileSaverBaseTests
{
    [Fact]
    public void Constructor_CreatesFileIfItDoesNotExist()
    {
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile);

        var fileSaver = new TripFileSaver(tempFile);

        Assert.True(File.Exists(tempFile));

        File.Delete(tempFile);
    }

    [Fact]
    public void AppendLine_AppendsTextWithNewline()
    {
        var tempFile = Path.GetTempFileName();
        var fileSaver = new TripFileSaver(tempFile);
        var testLine = "Test line";

        fileSaver.AppendLine(testLine);

        var fileContent = File.ReadAllText(tempFile);
        Assert.Equal(testLine + Environment.NewLine, fileContent);

        File.Delete(tempFile);
    }

    [Fact]
    public void AppendLine_MultipleAppends_ContainsAllLines()
    {
        var tempFile = Path.GetTempFileName();
        var fileSaver = new TripFileSaver(tempFile);
        var line1 = "First line";
        var line2 = "Second line";
        var line3 = "Third line";

        fileSaver.AppendLine(line1);
        fileSaver.AppendLine(line2);
        fileSaver.AppendLine(line3);

        var fileContent = File.ReadAllText(tempFile);
        Assert.Contains(line1 + Environment.NewLine, fileContent);
        Assert.Contains(line2 + Environment.NewLine, fileContent);
        Assert.Contains(line3 + Environment.NewLine, fileContent);

        File.Delete(tempFile);
    }
}

public class PackingListSaverTests
{
    [Fact]
    public void AppendData_FormatsAndAppendsPackingItem()
    {
        var tempFile = Path.GetTempFileName();
        var saver = new PackingListSaver(tempFile);
        var item = new PackingItem("T-shirt", 2, 5, ItemCategory.Clothing);

        saver.AppendData(item);

        var fileContent = File.ReadAllText(tempFile);
        var expectedLine = "T-shirt|2|5|Clothing" + Environment.NewLine;
        Assert.Equal(expectedLine, fileContent);

        File.Delete(tempFile);
    }

    [Fact]
    public void AppendData_MultipleItems_FormatsAllCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        var saver = new PackingListSaver(tempFile);
        var item1 = new PackingItem("T-shirt", 2, 5, ItemCategory.Clothing);
        var item2 = new PackingItem("Toothbrush", 1, 1, ItemCategory.Toiletries);
        var item3 = new PackingItem("Passport", 1, 1, ItemCategory.Documents);

        saver.AppendData(item1);
        saver.AppendData(item2);
        saver.AppendData(item3);

        var fileContent = File.ReadAllText(tempFile);
        Assert.Contains("T-shirt|2|5|Clothing" + Environment.NewLine, fileContent);
        Assert.Contains("Toothbrush|1|1|Toiletries" + Environment.NewLine, fileContent);
        Assert.Contains("Passport|1|1|Documents" + Environment.NewLine, fileContent);

        File.Delete(tempFile);
    }
}
