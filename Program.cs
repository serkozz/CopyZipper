using CommandLine;
using CommandLine.Text;

namespace CopyZipper;
class Program
{
    static Int32 Main(string[] args)
    {
        args = new String[] {
           "unzip",
           "-w",
           @"C:\Users\Сергей\Desktop\To",
           @"-t",
           @"C:\Users\Сергей\Desktop\To\ToInner",
           @"-o",
           @"-l",
           @"C:\Users\Сергей\DesktopLogs\log.txt",
        };
        var parser = new Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = null;
        });

        var parsedResults = parser.ParseArguments<UnzipOptions, CopyOptions>(args);

        return parsedResults.MapResult<UnzipOptions, CopyOptions, Int32>(
           (UnzipOptions unzipOptions) => Run(unzipOptions as IOptions),
           (CopyOptions copyOptions) => Run(copyOptions as IOptions),
            errors => DisplayHelp(parsedResults, errors)
        );
    }

    static Int32 DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "CopyZipper";
            h.Copyright = "Мониторит указанную папку на предмет новых архивов и распаковывает их по указанному пути";
            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        Console.WriteLine(helpText);
        return 0;
    }

    private static Int32 Run(IOptions opts)
    {
        return CopyZipper.WatchForChanges(opts);
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        if (errs.IsVersion())
        {
            Console.WriteLine("Version Request");
            return;
        }

        foreach (var err in errs)
        {
            Console.WriteLine($"Error occured: {err.Tag.ToString()}");
        }
    }
}
