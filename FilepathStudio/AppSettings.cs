using System.Text.Json.Serialization;

namespace FilepathStudio;

public class AppSettings
{
    [JsonPropertyName("lastOpenedFilePath")]
    public string? LastOpenedFilePath { get; set; }

    // 今後、ウィンドウのサイズや位置、フォントサイズなどの状態を追加しやすくしています。
    [JsonPropertyName("fontSize")]
    public double? FontSize { get; set; }
}
