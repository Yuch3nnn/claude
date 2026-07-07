namespace PacketMonitor.App.Models;

public enum PacketDirection
{
    Send,
    Receive,
    Unknown
}

public class PacketEntry
{
    public DateTime Timestamp { get; init; }
    public PacketDirection Direction { get; init; }
    public string Protocol { get; init; } = string.Empty;
    public string LocalEndPoint { get; init; } = string.Empty;
    public string RemoteEndPoint { get; init; } = string.Empty;
    public int Length { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public int Pid { get; init; }
    public byte[] Payload { get; init; } = [];

    public string DirectionText => Direction switch
    {
        PacketDirection.Send => "送出",
        PacketDirection.Receive => "接收",
        _ => "未知"
    };

    public string HexPreview => Convert.ToHexString(Payload.Length > 64 ? Payload[..64] : Payload);

    public string AsciiPreview
    {
        get
        {
            var chars = Payload.Select(b => b is >= 32 and < 127 ? (char)b : '.');
            return new string(chars.ToArray());
        }
    }
}
