using System.Reflection;
using DashLook.Common.Cross;

namespace DashLook.Linux;

public sealed class PluginManager
{
    private readonly List<IPreviewPlugin> _plugins = new();

    public void LoadPlugins()
    {
        // Register built-in Linux plugins
        _plugins.Add(new Plugins.ImagePlugin());
        _plugins.Add(new Plugins.TextPlugin());
        _plugins.Add(new Plugins.PdfPlugin());
        _plugins.Add(new Plugins.MarkdownPlugin());
        _plugins.Add(new Plugins.ArchivePlugin());
        _plugins.Add(new Plugins.FontPlugin());
        _plugins.Add(new Plugins.HtmlPlugin());
        _plugins.Add(new Plugins.VideoPlugin());

        _plugins.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public IPreviewPlugin? FindPlugin(string path)
    {
        foreach (var p in _plugins)
        {
            try { if (p.CanHandle(path)) return p; }
            catch { }
        }
        return null;
    }
}
