using System.IO;
using System.Reflection;
using DashLook.Common;

namespace DashLook.Services;

/// <summary>
/// Discovers and loads IViewer plugins from the Plugins\ directory at runtime.
/// Plugins are standard .NET assemblies that reference DashLook.Common.
/// Drop a new DLL into Plugins\ and it will be picked up on next launch.
/// </summary>
public sealed class PluginManager
{
    private readonly string _pluginsDir;
    private readonly List<IViewer> _viewers = new();

    public IReadOnlyList<IViewer> Viewers => _viewers;

    public PluginManager(string pluginsDir)
    {
        _pluginsDir = pluginsDir;
    }

    /// <summary>
    /// Scans the Plugins directory and also the current assembly for built-in viewers.
    /// </summary>
    public void LoadPlugins()
    {
        // In single-file publish, referenced assemblies are bundled but loaded lazily.
        // Force-load every DashLook.Plugin.* assembly so the types are discoverable.
        foreach (var asmName in Assembly.GetEntryAssembly()?.GetReferencedAssemblies()
                                ?? Array.Empty<AssemblyName>())
        {
            if (asmName.Name?.StartsWith("DashLook.Plugin.", StringComparison.Ordinal) == true)
                try { Assembly.Load(asmName); } catch { /* skip if unavailable */ }
        }

        // Portable (non-single-file): DLLs sit next to the EXE.
        foreach (var dll in Directory.GetFiles(AppContext.BaseDirectory, "DashLook.Plugin.*.dll"))
            try { Assembly.LoadFrom(dll); } catch { }

        // Discover viewers across all now-loaded assemblies
        LoadFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        // Load external plugins from Plugins\
        if (Directory.Exists(_pluginsDir))
        {
            foreach (var dll in Directory.GetFiles(_pluginsDir, "DashLook.Plugin.*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    LoadFromAssemblies(new[] { asm });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PluginManager] Failed to load {dll}: {ex.Message}");
                }
            }
        }

        // Sort by priority descending so highest-priority plugin wins
        _viewers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    private void LoadFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var asm in assemblies)
        {
            try
            {
                foreach (var type in asm.GetExportedTypes())
                {
                    if (!type.IsClass || type.IsAbstract) continue;
                    if (!typeof(IViewer).IsAssignableFrom(type)) continue;
                    if (type.GetCustomAttribute<ViewerPluginAttribute>() is null) continue;

                    var viewer = (IViewer?)Activator.CreateInstance(type);
                    if (viewer is not null)
                        _viewers.Add(viewer);
                }
            }
            catch { /* skip assemblies that fail inspection */ }
        }
    }

    /// <summary>
    /// Returns the best viewer for the given file path, or null if none found.
    /// </summary>
    public IViewer? FindViewer(string filePath)
    {
        foreach (var viewer in _viewers)
        {
            try
            {
                if (viewer.CanHandle(filePath))
                    return viewer;
            }
            catch { }
        }
        return null;
    }

    /// <summary>Returns metadata for all loaded plugins (for settings UI).</summary>
    public IEnumerable<(string Name, string Description, string Version)> GetPluginInfo()
    {
        foreach (var v in _viewers)
        {
            var attr = v.GetType().GetCustomAttribute<ViewerPluginAttribute>();
            if (attr is not null)
                yield return (attr.Name, attr.Description, attr.Version);
        }
    }
}
