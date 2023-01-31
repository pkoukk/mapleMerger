using WzMerger;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;

static void Merge()
{
    Console.WriteLine("Input the source wz file you want to use");
    string? sourceWzPath = Console.ReadLine();
    if (sourceWzPath == null || sourceWzPath == "")
    {
        //Console.WriteLine("empty file path");
        //return;
        sourceWzPath = "./files/base/String.wz";
    }
    Console.WriteLine("Input the override wz file you want to use");
    string? overrideWzPath = Console.ReadLine();
    if (overrideWzPath == null || overrideWzPath == "")
    {
        // Console.WriteLine("empty file path");
        // return;
        overrideWzPath = "./files/override/String.wz";
    }

    try
    {
        Merger merger = new Merger(sourceWzPath, overrideWzPath);
        Console.WriteLine("wz file loaded");
        Console.WriteLine("Input the wz path or txt path file you want to merge");
        string? path = Console.ReadLine();
        if (path == null || path == "")
        {
            // Console.WriteLine("empty path");
            // return;
            path = "String.wz/Etc.img";
        }
        merger.Merge(path);
        Console.WriteLine("merge finished");
        Console.WriteLine("Input the path you want to save");
        string? savePath = Console.ReadLine();
        if (savePath == null || savePath == "")
        {
            savePath = "./files/target/String.wz";
        }
        merger.Save(savePath);
        Console.WriteLine("save complete");
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

