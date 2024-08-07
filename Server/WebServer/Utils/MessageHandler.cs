using System.Globalization;
using CsvHelper;
using SharedCode.Model;


public class MessageHandler
{
    private readonly string _csvPath;
    private Dictionary<CompositeKey, string> _idToItem { get; set; }
    private readonly Timer _timer;
    private DateTime _lastModified;
    public MessageHandler(string csvPath)
    {
        _csvPath = csvPath;
        _idToItem = new Dictionary<CompositeKey, string>();

        ReadFile(csvPath);

        _timer = new Timer(CheckFileChanged, null, Timeout.Infinite, Timeout.Infinite);
        _timer.Change(0, 5000);
    }

    private void ReadFile(string csvPath)
    {
        _idToItem.Clear();
        using (var reader = new StreamReader(_csvPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<MessageModel>().ToList();
            foreach (var record in records)
            {
                if (record.TYPE == null || record.Message == null) continue;
                var key = new CompositeKey(record.TYPE, record.MessageCode);
                _idToItem[key] = record.Message;
            }
        }

        _lastModified = File.GetLastWriteTime(csvPath);
    }

    public string GetMessage(string type, int messageCode)
    {
        var key = new CompositeKey(type, messageCode);
        return _idToItem.TryGetValue(key, out var item) ? item : null;
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

