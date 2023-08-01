using CommandLine;
using CommandLine.Text;

namespace CopyZipper;
class Program
{
    static async Task Main(string[] args)
    {
        //args = new String[] {
        //    "-w",
        //    @"C:\Users\Сергей\Desktop\From",
        //    @"-t",
        //    @"C:\Users\Сергей\Desktop\To",
        //    @"-o",
        //};
        var parser = new Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = null;
        });
        var parsedResults = parser.ParseArguments<Options>(args);
        await(await parsedResults.WithParsedAsync(Run)).WithNotParsedAsync(errs => Task.FromResult<Int32>(DisplayHelp(parsedResults, errs)));
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

    private static async Task Run(Options opts)
    {
        await CopyZipper.WatchForChanges(opts);
        System.Console.WriteLine("Program Finished");
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
