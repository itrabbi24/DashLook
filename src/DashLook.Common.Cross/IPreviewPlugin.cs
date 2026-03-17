namespace DashLook.Common.Cross;

/// <summary>
/// Cross-platform preview plugin contract.
/// Windows plugins use DashLook.Common (WPF UIElement).
/// Linux plugins use this interface (Avalonia Control).
/// </summary>
public interface IPreviewPlugin
{
    int Priority { get; }
    bool CanHandle(string path);
    Task<PreviewResult> PrepareAsync(string path, CancellationToken token);
    void Cleanup();
}

public sealed class PreviewResult
{
    public bool Success    { get; init; }
    public string? Error   { get; init; }
    public object? Control { get; init; }   // Avalonia Control on Linux, WPF UIElement on Windows
    public string FileType { get; init; } = "";
    public string FileSize { get; init; } = "";
    public (double Width, double Height) PreferredSize { get; init; } = (900, 620);
}
