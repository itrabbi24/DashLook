namespace DashLook.Common;

/// <summary>
/// Decorate your IViewer implementation class with this attribute
/// so PluginManager can discover it via reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ViewerPluginAttribute : Attribute
{
    /// <summary>Human-readable plugin name shown in settings.</summary>
    public string Name { get; }

    /// <summary>Short description of what the plugin previews.</summary>
    public string Description { get; }

    /// <summary>Semantic version string, e.g. "1.0.0".</summary>
    public string Version { get; }

    public ViewerPluginAttribute(string name, string description, string version = "1.0.0")
    {
        Name = name;
        Description = description;
        Version = version;
    }
}
