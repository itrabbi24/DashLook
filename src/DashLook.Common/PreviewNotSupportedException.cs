namespace DashLook.Common;

/// <summary>
/// Throw this from IViewer.PrepareAsync to tell the host that this plugin
/// cannot handle the file after inspection, so the next plugin should try.
/// </summary>
public class PreviewNotSupportedException : Exception
{
    public PreviewNotSupportedException() : base("Preview not supported for this file.") { }
    public PreviewNotSupportedException(string message) : base(message) { }
    public PreviewNotSupportedException(string message, Exception inner) : base(message, inner) { }
}
