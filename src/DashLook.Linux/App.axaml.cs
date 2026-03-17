using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace DashLook.Linux;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var args = desktop.Args ?? Array.Empty<string>();

            // Can be launched with a file path as argument:
            //   dashlook /path/to/file.png
            // Or with no args to show the tray icon and wait for file manager integration
            if (args.Length > 0 && File.Exists(args[0]))
            {
                var pluginManager = new PluginManager();
                pluginManager.LoadPlugins();

                var win = new PreviewWindow(args[0], pluginManager);
                desktop.MainWindow = win;
            }
            else
            {
                // Launch as tray/daemon, wait for file manager to call us
                // Tray support coming in v1.1
                Console.WriteLine("DashLook — pass a file path as argument to preview it.");
                Console.WriteLine("Usage: dashlook <file>");
                desktop.Shutdown();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
