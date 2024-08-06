using System.Globalization;
using System.IO;
using CsvHelper;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class DataHandler<T> where T : class
{
    private readonly string _csvPath;
    private readonly string _idPropertyName;
    protected Dictionary<int, T> _idToItem { get; private set; }
    private readonly Timer _timer;
    private DateTime _lastModified;

    public DataHandler(string csvPath, string idPropertyName)
    {
        _csvPath = csvPath;
        _idPropertyName = idPropertyName;
        _idToItem = new Dictionary<int, T>();

        ReadFile(csvPath);

        _timer = new Timer(CheckFileChanged, null, Timeout.Infinite, Timeout.Infinite);
        _timer.Change(0, 5000);
    }

    private void ReadFile(string csvPath)
    {
        _idToItem.Clear();
        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<T>().ToList();
            foreach (var record in records)
            {
                var property = record.GetType().GetProperty(_idPropertyName);
                if (property != null)
                {
                    var value = property.GetValue(record);
                    if (value != null && value is int itemId)
                    {
                        _idToItem.Add(itemId, record);
                    }
                    else
                    {
                        // Handle the case where the ID property is null or not an int
                        throw new InvalidOperationException($"Invalid or null value for {_idPropertyName} in record.");
                    }
                }
                else
                {
                    // Handle the case where the property is not found
                    throw new InvalidOperationException($"Property {_idPropertyName} not found in record.");
                }
            }
        }

        _lastModified = File.GetLastWriteTime(csvPath);
    }


    protected T GetItemById(int itemId)
    {
        return _idToItem.TryGetValue(itemId, out var item) ? item : null;
    }

    private void CheckFileChanged(object state)
    {
        DateTime currentModified = File.GetLastWriteTime(_csvPath);
        if (currentModified != _lastModified)
        {
            ReadFile(_csvPath);
        }
    }
}
