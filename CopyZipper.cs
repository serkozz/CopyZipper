using System.IO.Compression;
using System.Text.RegularExpressions;
using Aspose.Zip.SevenZip;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace CopyZipper;
static class CopyZipper
{
    public static Int32 WatchForChanges(IOptions options)
    {
        FileSystemWatcher watcher = new FileSystemWatcher()
        {
            Path = options.WatchPath,
        };

        Logger logger = new Logger();

        if (!String.IsNullOrEmpty(options.LogPath?.Trim()))
            logger.ConfigureFileLogger(options.LogPath!,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            rollingInterval: Serilog.RollingInterval.Day);

        logger.ConfigureConsole();
        logger.CreateLogger();

        watcher.Created += (sender, args) =>
        {
            OnNewFileDetected(args, options, logger);
        };
        watcher.Renamed += (sender, args) =>
        {
            OnNewFileDetected(args, options, logger);
        };

        watcher.EnableRaisingEvents = true;

        if (options is UnzipOptions)
            logger.Log(Serilog.Events.LogEventLevel.Information, $"CopyZipper started! Mode: Unzip. Watching '{options.WatchPath}' folder");
        else if (options is CopyOptions)
            logger.Log(Serilog.Events.LogEventLevel.Information, $"CopyZipper started! Mode: Copy. Watching '{options.WatchPath}' folder");

        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    private static void OnNewFileDetected(FileSystemEventArgs e, IOptions options, Logger logger)
    {
        try
        {
            string[] nameExtPair = e.Name!.Split('.', 2);

            Boolean regexMatch = Regex.IsMatch(nameExtPair[0], options.FileNameRegex!, RegexOptions.IgnoreCase);
            Boolean extMatch = nameExtPair.Length == 2 && options.FileExtension.GetExtension().ToLower().Contains(nameExtPair[1].ToLower());

            if (regexMatch && extMatch)
            {
                logger.Log(Serilog.Events.LogEventLevel.Information, $"Created File '{e.FullPath}'");

                if (options is UnzipOptions)
                    Unzip(e.FullPath, (options as UnzipOptions)!, logger);
                else if (options is CopyOptions)
                    Copy(e.FullPath, (options as CopyOptions)!, logger);

                if (options.DeleteAfter)
                {
                    File.Delete(e.FullPath);
                    logger.Log(Serilog.Events.LogEventLevel.Verbose, $"Original archive was deleted!");
                }
            }
            else if (regexMatch && !extMatch)
                throw new ArgumentException($"Extension missmatch (Watching for {options.FileExtension.ToString().ToUpper()} but found {nameExtPair[1].ToUpper()})");
            else
                throw new ArgumentException($"Regex missmatch");
        }
        catch (Exception ex)
        {
            logger.LogException(Serilog.Events.LogEventLevel.Error, ex, $"Error: {ex.Message}");
        }
    }

    private static void Copy(String fullPath, CopyOptions copyOptions, Logger logger)
    {
        foreach (var toPath in copyOptions.ToPaths)
        {
            logger.Log(Serilog.Events.LogEventLevel.Information, $"Copying '{fullPath}' to '{toPath}' folder");
            var filenameSplitted = Path.GetFileName(fullPath).Split('.', 2);
            logger.LogExecutionTime(
                () => {
                    File.Copy(fullPath, toPath + Path.DirectorySeparatorChar + filenameSplitted[0] + ".temp", copyOptions.Override);
                    File.Move(toPath + Path.DirectorySeparatorChar + filenameSplitted[0] + ".temp",
                    toPath + Path.DirectorySeparatorChar + String.Join('.', filenameSplitted));
                },
                $"Copying '{fullPath}' to '{toPath}' folder");
        }
    }

    private static void Unzip(String fullPath, UnzipOptions options, Logger logger)
    {
        UnzipOptions? currentOptions;

        if (options.FileExtension == FileExtension.All)
        {
            string extension = Path.GetExtension(fullPath).TrimStart('.');
            currentOptions = options.Clone() as UnzipOptions;
            ArgumentNullException.ThrowIfNull(currentOptions);
            if (currentOptions is null)
                throw new ArgumentNullException("Cant cast object to options");
            currentOptions.FileExtension = extension.ToExtension();
        }
        else
            currentOptions = options;

        foreach (var toPath in options.ToPaths)
        {
            logger.Log(Serilog.Events.LogEventLevel.Information, $"Unziping '{fullPath}' to '{toPath}' folder");
            logger.LogExecutionTime(() => UnzipSwitch(fullPath, toPath, currentOptions, options.Override),
                $"Unziping '{fullPath}' to '{toPath}' folder");
        }
    }

    private static void UnzipSwitch(String fullPath, String toPath, UnzipOptions currentOptions, Boolean overrideResult)
    {
        switch (currentOptions.FileExtension)
        {
            case FileExtension.Zip:
                {
                    ZipFile.ExtractToDirectory(fullPath, toPath, currentOptions.Override);
                    break;
                }
            case FileExtension.Rar:
                {
                    using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(fullPath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(toPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = currentOptions.Override
                            });
                        }
                    }
                    break;
                }
            case FileExtension.SevenZip:
                {
                    using (SevenZipArchive archive = new SevenZipArchive(fullPath))
                    {
                        var resultDirectory = toPath + Path.DirectorySeparatorChar + archive.Entries.First().Name;
                        if (!overrideResult && Directory.Exists(resultDirectory))
                            throw new Exception($"Directory '{resultDirectory}' already exists");
                        archive.ExtractToDirectory(toPath);
                    }
                    break;
                }
            default:
                throw new ArgumentException("Unsupported archive extension");
        }
    }
}