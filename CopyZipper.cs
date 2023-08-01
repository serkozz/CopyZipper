using System.IO.Compression;
using System.Text.RegularExpressions;
using Aspose.Zip.SevenZip;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace CopyZipper;
static class CopyZipper
{
    public static async Task WatchForChanges(Options options)
    {
        FileSystemWatcher watcher = new FileSystemWatcher()
        {
            Path = options.WatchPath,
        };

        watcher.Created += (sender, args) =>
        {
            OnNewFileDetected(args, options);
        };
        watcher.Renamed += (sender, args) =>
        {
            OnNewFileDetected(args, options);
        };

        watcher.EnableRaisingEvents = true;
        while (true)
        {
            await Task.Delay(1000);
            System.Console.WriteLine($"{DateTime.Now} === (CopyZipper) Watching...");
        }
    }

    private static void OnNewFileDetected(FileSystemEventArgs e, Options options)
    {
        try
        {
            string[] nameExtPair = e.Name!.Split('.', 2);

            Boolean regexMatch = Regex.IsMatch(nameExtPair[0], options.FileNameRegex!, RegexOptions.IgnoreCase);
            Boolean extMatch = nameExtPair.Length == 2 && options.FileExtension.GetExtension().ToLower().Contains(nameExtPair[1].ToLower());

            if (regexMatch && extMatch)
            {
                Console.WriteLine($"{Environment.NewLine}-----------------------------------");
                Console.WriteLine($"Created File \"{nameExtPair[0] + '.' + nameExtPair[1]}\"");
                Console.WriteLine($"Unziping to \"{options.ToPath}\" folder");
                Unzip(e.FullPath, options);
                Console.WriteLine($"Done!");
                if (options.DeleteAfter)
                {
                    Console.WriteLine($"Original archive was deleted!");
                    File.Delete(e.FullPath);
                }
                Console.WriteLine($"-----------------------------------");
            }
            else if (regexMatch && !extMatch)
                throw new ArgumentException($"Extension missmatch (Watching for {options.FileExtension.ToString().ToUpper()} but found {nameExtPair[1].ToUpper()})");
            else
                throw new ArgumentException($"Regex missmatch");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Error occured: " + ex.Message);
        }
    }

    private static void Unzip(String fullPath, Options options)
    {
        Options? currentOptions;
        if (options.FileExtension == FileExtension.All)
        {
            string extension = Path.GetExtension(fullPath).TrimStart('.');
            currentOptions = options.Clone();
            if (currentOptions is null)
                throw new ArgumentNullException("Cant cast object to options");
            currentOptions.FileExtension = extension.ToExtension();
        }
        else
            currentOptions = options;

        switch (currentOptions.FileExtension)
        {
            case FileExtension.Zip:
                {
                    ZipFile.ExtractToDirectory(fullPath, currentOptions.ToPath, currentOptions.Override);
                    break;
                }
            case FileExtension.Rar:
                {
                    using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(fullPath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(currentOptions.ToPath, new ExtractionOptions()
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
                        var resultDirectory = currentOptions.ToPath + Path.DirectorySeparatorChar + archive.Entries.First().Name;
                        if (!options.Override && Directory.Exists(resultDirectory))
                            throw new Exception($"Directory '{resultDirectory}' already exists");
                        archive.ExtractToDirectory(currentOptions.ToPath);
                    }
                    break;
                }
            default:
                throw new ArgumentException("Unsupported archive extension");
        }
    }
}