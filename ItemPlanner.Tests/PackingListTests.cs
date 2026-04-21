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
        var nonExistentFile = "nonexistentname.txt";


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
        string fileName = "myTestFile.txt";
        string path = Directory.GetCurrentDirectory() + "/PackingLists/" + "myTestFile.txt";
        var tempFile = path;
        var testData = "T-shirt|2|5|Clothing\nToothbrush|1|1|Toiletries\nPassport|1|1|Documents";
        File.WriteAllText(path, testData);
        var manager = new ItemListManager();

        manager.ReadFile(fileName);

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
    public void ReadFile_SetsItemListDataToEmpty_WhenFileDoesNotExist()
    {
        var manager = new ItemListManager();
        var nonExistentFile = "nonexistentTest.txt";

        manager.ReadFile(nonExistentFile);

        Assert.Empty(manager.itemListData);
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

public class TripFileSaverTests
{
    [Fact]
    public void AppendData_FormatsAndAppendsTripData()
    {
        var tempFile = Path.GetTempFileName();
        var saver = new TripFileSaver(tempFile);
        var tripData = new TripData(DateTime.Parse("2026-04-20"), "Paris", "Sightseeing", "presetlist1.txt");

        saver.AppendData(tripData);

        var fileContent = File.ReadAllText(tempFile);
        var expectedLine = "04/20/2026 00:00:00::Paris::Sightseeing::presetlist1.txt" + Environment.NewLine;
        Assert.Equal(expectedLine, fileContent);

        File.Delete(tempFile);
    }

    [Fact]
    public void AppendData_MultipleTrips_FormatsAllCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        var saver = new TripFileSaver(tempFile);
        var trip1 = new TripData(DateTime.Parse("2026-04-20"), "Paris", "Sightseeing", "presetlist1.txt");
        var trip2 = new TripData(DateTime.Parse("2026-05-01"), "Beach", "Beach Trip", "custom.txt");

        saver.AppendData(trip1);
        saver.AppendData(trip2);

        var fileContent = File.ReadAllText(tempFile);
        Assert.Contains("04/20/2026 00:00:00::Paris::Sightseeing::presetlist1.txt" + Environment.NewLine, fileContent);
        Assert.Contains("05/01/2026 00:00:00::Beach::Beach Trip::custom.txt" + Environment.NewLine, fileContent);

        File.Delete(tempFile);
    }
}

public class AdditionalFileSaverBaseTests
{
    [Fact]
    public void DeleteFile_DeletesExistingFile()
    {
        var tempFile = Path.GetTempFileName();
        var fileSaver = new TripFileSaver(tempFile);

        // Verify file exists
        Assert.True(File.Exists(tempFile));

        // Delete the file
        fileSaver.DeleteFile(tempFile);

        // Verify file is deleted
        Assert.False(File.Exists(tempFile));
    }
}

public class AdditionalItemListManagerTests
{
    [Fact]
    public void ReWriteFile_RewritesAllItemsToFile()
    {
        var manager = new ItemListManager();
        var tempFile = "test_rewrite.txt";
        var fullPath = manager.absFilePath + "/" + tempFile;

        // Create some test data
        manager.itemListData = new List<PackingItem>
        {
            new PackingItem("T-shirt", 2, 5, ItemCategory.Clothing),
            new PackingItem("Toothbrush", 1, 1, ItemCategory.Toiletries)
        };

        // Rewrite the file
        manager.ReWriteFile(tempFile);

        // Verify file contents
        Assert.True(File.Exists(fullPath));
        var fileContent = File.ReadAllText(fullPath);
        Assert.Contains("T-shirt|2|5|Clothing" + Environment.NewLine, fileContent);
        Assert.Contains("Toothbrush|1|1|Toiletries" + Environment.NewLine, fileContent);

        File.Delete(fullPath);
    }

    [Fact]
    public void ReWriteFile_DoesNothing_WhenItemListDataIsNull()
    {
        var manager = new ItemListManager();
        var tempFile = "test_rewrite_null.txt";
        var fullPath = manager.absFilePath + "/" + tempFile;

        // Set itemListData to null
        manager.itemListData = null;

        // Rewrite the file
        manager.ReWriteFile(tempFile);

        // Verify file exists but is empty
        Assert.True(File.Exists(fullPath));
        var fileContent = File.ReadAllText(fullPath);
        Assert.Equal("", fileContent);

        File.Delete(fullPath);
    }

    [Fact]
    public void DeletePackingList_DeletesFile_WhenFileExists()
    {
        var manager = new ItemListManager();
        var tempFile = "test_delete.txt";
        var fullPath = manager.absFilePath + "/" + tempFile;

        // Create a file
        File.WriteAllText(fullPath, "test content");

        // Verify file exists
        Assert.True(File.Exists(fullPath));

        // Delete the packing list
        manager.DeletePackingList(tempFile);

        // Verify file is deleted
        Assert.False(File.Exists(fullPath));
    }
}

public class AdditionalDataManagerTests
{
    [Fact]
    public void Constructor_HandlesEmptyFile()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, ""); // Empty file

        var dataManager = new DataManager(tempFile);

        Assert.Empty(dataManager.TripData);

        File.Delete(tempFile);
    }

    [Fact]
    public void Constructor_HandlesFileWithEmptyLines()
    {
        var tempFile = Path.GetTempFileName();
        var testData = "2026-04-20::Paris::Sightseeing::presetlist1.txt\n\n2026-05-01::Beach::Beach Trip::custom.txt\n";
        File.WriteAllText(tempFile, testData);

        var dataManager = new DataManager(tempFile);

        Assert.Equal(2, dataManager.TripData.Count);
        Assert.Equal("Paris", dataManager.TripData[0].Destination);
        Assert.Equal("Beach", dataManager.TripData[1].Destination);

        File.Delete(tempFile);
    }

    [Fact]
    public void DefaultConstructor_UsesDefaultFileName()
    {
        var dataManager = new DataManager();

        Assert.Equal("trip-data.txt", dataManager.tripFileName);
    }
}

public class AdditionalTripDataTests
{
    [Fact]
    public void ToString_FormatsDateCorrectly()
    {
        var date = DateTime.Parse("2026-12-25");
        var tripData = new TripData(date, "New York", "Holiday", "holiday.txt");

        var result = tripData.ToString();

        Assert.Equal("Holiday at New York on 2026-12-25", result);
    }

    [Fact]
    public void ToString_HandlesDifferentDateFormats()
    {
        var date = DateTime.Parse("2026-01-01");
        var tripData = new TripData(date, "Tokyo", "New Year", "ny.txt");

        var result = tripData.ToString();

        Assert.Equal("New Year at Tokyo on 2026-01-01", result);
    }
}

public class AdditionalPackingItemTests
{
    [Fact]
    public void Constructor_HandlesZeroQuantities()
    {
        var item = new PackingItem("Empty Item", 0, 0, ItemCategory.Misc);

        Assert.Equal("Empty Item", item.Name);
        Assert.Equal(0, item.QuantityPacked);
        Assert.Equal(0, item.QuantityToPack);
        Assert.Equal(ItemCategory.Misc, item.Category);
        Assert.True(item.IsFullyPacked); // 0 >= 0 is true
    }

    [Fact]
    public void Constructor_HandlesLargeQuantities()
    {
        var item = new PackingItem("Bulk Item", 1000, 500, ItemCategory.Clothing);

        Assert.Equal("Bulk Item", item.Name);
        Assert.Equal(1000, item.QuantityPacked);
        Assert.Equal(500, item.QuantityToPack);
        Assert.Equal(ItemCategory.Clothing, item.Category);
        Assert.True(item.IsFullyPacked); // 1000 >= 500 is true
    }

    [Theory]
    [InlineData("Electronics", ItemCategory.Electronics)]
    [InlineData("Documents", ItemCategory.Documents)]
    [InlineData("Misc", ItemCategory.Misc)]
    public void Constructor_SetsAllCategoriesCorrectly(string name, ItemCategory category)
    {
        var item = new PackingItem(name, 1, 1, category);

        Assert.Equal(category, item.Category);
        Assert.True(item.IsFullyPacked);
    }
}
