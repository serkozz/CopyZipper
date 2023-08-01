using CommandLine;

public class Options
{
    [Option('w', "watch", Required = true, HelpText = "Путь к папке, в которой необходимо искать архивы")]
    public required String WatchPath { get; set; }

    [Option('t', "to", Required = true, HelpText = "Путь к папке, в которую будут помещены распакованные архивы")]
    public required String ToPath { get; set; }

    [Option('r', "regex", HelpText = "Регулярное выражение, определяющее наименование файлов (по умолчанию опеределяет корректные даты формата дд{.|/|-}мм{.|/|-}гггг)",
    Default = @"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[13-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$")]
    public String? FileNameRegex { get; set; }

    [Option('e', "ext", HelpText = "Расширение файлов", Default = FileExtension.All)]
    public FileExtension FileExtension { get; set; }
    [Option('o', "override", HelpText = "Перезаписывать файлы при распаковке?", Default = false)]
    public Boolean Override { get; set; }

    [Option('d', "delete", HelpText = "Удалить исходный архив после распаковки?", Default = false)]
    public Boolean DeleteAfter { get; set; }

    public Options? Clone() => this.MemberwiseClone() as Options;
}

[Flags]
public enum FileExtension
{
    Zip,
    Rar,
    SevenZip,
    All
}

static class FileExtensionExtensions
{
    public static FileExtension ToExtension(this string extensionString)
    {
        switch (extensionString)
        {
            case "rar":
                return FileExtension.Rar;
            case "zip":
                return FileExtension.Zip;
            case "7z":
                return FileExtension.SevenZip;
            default:
                return FileExtension.All;
        }
    }
    public static string GetExtension(this FileExtension extension)
    {
        var Exceptions = new Dictionary<FileExtension, string>() {
                                        { FileExtension.SevenZip, "7z" },
                                        { FileExtension.All, String.Join("::", Enum.GetNames(typeof(FileExtension))).Replace("SevenZip", "7z") },
                                    };
        if (Exceptions.ContainsKey(extension))
        {
            return Exceptions[extension];
        }
        else
        {
            return extension.ToString();
        }
    }
}
