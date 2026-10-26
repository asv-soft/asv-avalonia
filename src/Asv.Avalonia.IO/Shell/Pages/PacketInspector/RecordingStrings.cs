namespace Asv.Avalonia.IO;

public static class RecordingStrings
{
    public static string Title => RS.ResourceManager.GetString("PacketRecording_Title", RS.Culture) ?? "Recording";
    public static string Format => RS.ResourceManager.GetString("PacketRecording_Format", RS.Culture) ?? "File format";
    public static string Binary => RS.ResourceManager.GetString("PacketRecording_Binary", RS.Culture) ?? "Binary (.bin)";
    public static string JsonLines => RS.ResourceManager.GetString("PacketRecording_JsonLines", RS.Culture) ?? "JSONL (.jsonl)";
    public static string Start => RS.ResourceManager.GetString("PacketRecording_Start", RS.Culture) ?? "Start recording";
    public static string Stop => RS.ResourceManager.GetString("PacketRecording_Stop", RS.Culture) ?? "Stop recording";
    public static string Preparing => RS.ResourceManager.GetString("PacketRecording_Preparing", RS.Culture) ?? "Preparing recording…";
    public static string Stopping => RS.ResourceManager.GetString("PacketRecording_Stopping", RS.Culture) ?? "Finishing recording…";
    public static string Save => RS.ResourceManager.GetString("PacketRecording_Save", RS.Culture) ?? "Save traffic recording";
    public static string Hint => RS.ResourceManager.GetString("PacketRecording_Hint", RS.Culture) ?? "Records all new RX/TX messages, independently of view filters.";
    public static string Counters => RS.ResourceManager.GetString("PacketRecording_Counters", RS.Culture) ?? "{0:N0} messages · {1:N0} bytes";
    public static string BufferFull => RS.ResourceManager.GetString("PacketRecording_BufferFull", RS.Culture) ?? "Recording stopped: the disk cannot keep up with traffic.";
}
