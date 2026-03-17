using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DashLook.Common;

namespace DashLook.Plugin.FolderViewer;

[ViewerPlugin("Folder Viewer", "Shows a quick summary for folders", "1.0.0")]
public sealed class FolderViewerPlugin : IViewer
{
    public int Priority => 100;

    private FolderViewerControl? _control;
    public UIElement? ViewerControl => _control;

    public bool CanHandle(string path) => Directory.Exists(path);

    public async Task PrepareAsync(string path, ContextObject context, CancellationToken token)
    {
        var info = await Task.Run(() => BuildInfo(path, token), token);
        token.ThrowIfCancellationRequested();

        context.FileType = "Folder";
        context.PreferredSize = new Size(920, 430);
        context.FileSize = info.SizeText;
        _control = new FolderViewerControl(info);
    }

    public void Cleanup() => _control = null;

    private static FolderPreviewInfo BuildInfo(string path, CancellationToken token)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists)
            throw new PreviewNotSupportedException("Folder no longer exists.");

        int folderCount = 0;
        int fileCount = 0;
        long totalBytes = 0;

        var pending = new Stack<DirectoryInfo>();
        pending.Push(dir);

        while (pending.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            DirectoryInfo current = pending.Pop();

            var childDirs = EnumerateChildDirectories(current);
            var childFiles = EnumerateChildFiles(current);

            foreach (var childDir in childDirs)
            {
                folderCount++;

                if (!ShouldTraverseDirectory(childDir))
                    continue;

                pending.Push(childDir);
            }

            foreach (var childFile in childFiles)
            {
                fileCount++;
                totalBytes += childFile.Length;
            }
        }

        string modifiedText = dir.LastWriteTime.ToString("yyyy/M/d h:mm:ss tt");
        string summary = $"{ContextObject.FormatFileSize(totalBytes)} ({folderCount} folders and {fileCount} files)";

        return new FolderPreviewInfo(
            dir.Name,
            modifiedText,
            folderCount,
            fileCount,
            ContextObject.FormatFileSize(totalBytes),
            summary);
    }

    private static List<DirectoryInfo> EnumerateChildDirectories(DirectoryInfo directory) =>
        EnumerateSafe(() => directory.EnumerateDirectories());

    private static List<FileInfo> EnumerateChildFiles(DirectoryInfo directory) =>
        EnumerateSafe(() => directory.EnumerateFiles());

    private static List<T> EnumerateSafe<T>(Func<IEnumerable<T>> factory)
    {
        try
        {
            return factory().ToList();
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (PathTooLongException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (IOException)
        {
        }

        return new List<T>();
    }

    private static bool ShouldTraverseDirectory(DirectoryInfo directory)
    {
        try
        {
            return !directory.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (PathTooLongException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (IOException)
        {
        }

        return false;
    }
}

public readonly record struct FolderPreviewInfo(
    string Name,
    string LastModifiedText,
    int FolderCount,
    int FileCount,
    string SizeText,
    string Summary);
