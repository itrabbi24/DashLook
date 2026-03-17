using System.IO;
using System.Reflection;
using DashLook.Common;

namespace DashLook.Services;

/// <summary>
/// Discovers and loads IViewer plugins from built-in plugin assemblies and the external Plugins directory.
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

    public void LoadPlugins()
    {
        _viewers.Clear();
        var discoveredTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var asmName in Assembly.GetEntryAssembly()?.GetReferencedAssemblies() ?? Array.Empty<AssemblyName>())
        {
            if (asmName.Name?.StartsWith("DashLook.Plugin.", StringComparison.Ordinal) != true)
                continue;

            try
            {
                Assembly.Load(asmName);
            }
            catch (Exception ex)
            {
                LogService.Write($"Failed to load bundled plugin assembly {asmName.Name}: {ex.Message}");
            }
        }

        foreach (var dll in Directory.GetFiles(AppContext.BaseDirectory, "DashLook.Plugin.*.dll"))
        {
            try
            {
                Assembly.LoadFrom(dll);
            }
            catch (Exception ex)
            {
                LogService.Write($"Failed to load built-in plugin DLL {dll}: {ex.Message}");
            }
        }

        LoadFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), discoveredTypes);

        if (Directory.Exists(_pluginsDir))
        {
            foreach (var dll in Directory.GetFiles(_pluginsDir, "DashLook.Plugin.*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    LoadFromAssemblies(new[] { asm }, discoveredTypes);
                }
                catch (Exception ex)
                {
                    LogService.Write($"Failed to load external plugin DLL {dll}: {ex.Message}");
                }
            }
        }

        _viewers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        LogService.Write($"PluginManager loaded {_viewers.Count} viewer(s).");
    }

    private void LoadFromAssemblies(IEnumerable<Assembly> assemblies, HashSet<string> discoveredTypes)
    {
        foreach (var asm in assemblies)
        {
            try
            {
                foreach (var type in asm.GetExportedTypes())
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    if (!typeof(IViewer).IsAssignableFrom(type))
                        continue;
                    if (type.GetCustomAttribute<ViewerPluginAttribute>() is null)
                        continue;
                    if (!discoveredTypes.Add(type.AssemblyQualifiedName ?? type.FullName ?? type.Name))
                        continue;

                    var viewer = (IViewer?)Activator.CreateInstance(type);
                    if (viewer is null)
                        continue;

                    _viewers.Add(viewer);
                    LogService.Write($"Loaded viewer: {type.FullName}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                string details = string.Join(" | ", ex.LoaderExceptions.Where(e => e is not null).Select(e => e!.Message));
                LogService.Write($"Failed to inspect assembly {asm.FullName}: {details}");
            }
            catch (Exception ex)
            {
                LogService.Write($"Failed to inspect assembly {asm.FullName}: {ex.Message}");
            }
        }
    }

    public IViewer? FindViewer(string filePath)
    {
        foreach (var viewer in _viewers)
        {
            try
            {
                if (viewer.CanHandle(filePath))
                    return viewer;
            }
            catch (Exception ex)
            {
                LogService.Write($"Viewer {viewer.GetType().FullName} failed CanHandle('{filePath}'): {ex.Message}");
            }
        }

        return null;
    }

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
