using WzMerger;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;

static void Merge()
{
    Console.WriteLine("Input the base wz directory you want to use");
    string? sourceWzPath = Console.ReadLine();
    if (sourceWzPath == null || sourceWzPath == "")
    {
        //Console.WriteLine("empty file path");
        //return;
        sourceWzPath = "/Users/yohn/workspace/playground/ms/data/wz/hvn";
    }
    Console.WriteLine("Input the override wz directory you want to use");
    string? overrideWzPath = Console.ReadLine();
    if (overrideWzPath == null || overrideWzPath == "")
    {
        // Console.WriteLine("empty file path");
        // return;
        overrideWzPath = "/Users/yohn/workspace/playground/ms/data/wz/079sdo";
    }

    try
    {
        Console.WriteLine("Input the list file you want to merge");
        string? path = Console.ReadLine();
        if (path == null || path == "")
        {
            // Console.WriteLine("empty path");
            // return;
            path = "/Users/yohn/workspace/playground/ms/data/others/b.txt";
        }
        Console.WriteLine("Input the directory you want to save");
        string? savePath = Console.ReadLine();
        if (savePath == null || savePath == "")
        {
            savePath = "/Users/yohn/workspace/playground/ms/data/wz/merged";
        }
        Merger merger = new Merger(sourceWzPath, overrideWzPath);
        merger.Merge(path, savePath);
        Console.WriteLine("merge finished");
    }
    catch (Exception ex)
    {
        Console.WriteLine("wz file merge failed :" + ex.Message);
        return;
    }
}

static void Export()
{
    Console.WriteLine("Input the wz file you want to export");
    string? wzPath = Console.ReadLine();
    if (wzPath == null || wzPath == "")
    {
        // Console.WriteLine("empty file path");
        // return;
        wzPath = "./files/target/String.wz";
    }
    Console.WriteLine("Input the directory you want to save");
    string? savePath = Console.ReadLine();
    if (savePath == null || savePath == "")
    {
        // Console.WriteLine("empty path");
        // return;
        savePath = "./files/target/xmls";
    }
    try
    {
        short fileVersion = 0;
        var gameVersion = WzTool.DetectMapleVersion(wzPath, out fileVersion);
        WzFile f = new WzFile(wzPath, fileVersion, gameVersion);
        var parseResult = f.ParseWzFile();
        if (parseResult == WzFileParseStatus.Success)
        {
            f.ExportXml(savePath, true);
            Console.WriteLine("export complete");
        }
        else
        {
            throw new Exception(WzFileParseStatusExtension.GetErrorDescription(parseResult));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("wz file export failed :" + ex.Message);
        return;
    }
}


Console.WriteLine("Press M to merge wz file");
Console.WriteLine("Press E to Export wz xml file");
var key = Console.ReadKey();
switch (key.Key)
{
    case ConsoleKey.M:
        Merge();
        break;
    case ConsoleKey.E:
        Export();
        break;
    default:
        break;
}

Console.WriteLine("Press any key...");
Console.ReadKey();

