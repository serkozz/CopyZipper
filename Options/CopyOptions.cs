using CommandLine;

[Verb("copy", HelpText = "Копирует архив из входной папки в выходную без разархивирования")]
public class CopyOptions : IOptions
{
    [Option('w', "watch", Required = true, HelpText = "Путь к папке, в которой необходимо искать архивы")]
    public required String WatchPath { get; set; }

    [Option('t', "to", Required = true, HelpText = "Путь к папке(ам), в которую(ые) будут помещены распакованные архивы")]
    public required IEnumerable<String> ToPaths { get; set; }

    [Option('r', "regex", HelpText = "Регулярное выражение, определяющее наименование файлов (по умолчанию опеределяет корректные даты формата дд{.|/|-}мм{.|/|-}гггг)",
    Default = @"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[13-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$")]
    public String? FileNameRegex { get; set; }

    [Option('e', "ext", HelpText = "Расширение файлов", Default = FileExtension.All)]
    public FileExtension FileExtension { get; set; }

    [Option('o', "override", HelpText = "Перезаписывать файлы?", Default = false)]
    public Boolean Override { get; set; }

    [Option('d', "delete", HelpText = "Удалить исходный архив после операций?", Default = false)]
    public Boolean DeleteAfter { get; set; }

    [Option('l', "log", HelpText = "Путь до файла, с логами (если файл не существует, он создается автоматически)")]
    public String? LogPath { get; set; }

    public Object? Clone() => this.MemberwiseClone() as CopyOptions;
}