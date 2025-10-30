namespace KintoneDiscordConnector.Models;

public class KintoneWebhookPayload
{
    public string Type { get; set; } = "";
    public AppInfo App { get; set; } = new();
    public RecordInfo Record { get; set; } = new();
    public string Url { get; set; } = "";
}

public class AppInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class RecordInfo
{
    public string Id { get; set; } = "";
    public UserInfo Modifier { get; set; } = new();
    public UserInfo Creator { get; set; } = new();
    public Dictionary<string, FieldValue> Fields { get; set; } = new();
}

public class UserInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public class FieldValue
{
    public string Type { get; set; } = "";
    public object? Value { get; set; }
}
