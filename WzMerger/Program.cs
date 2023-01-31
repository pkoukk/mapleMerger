using WzMerger;

static void Run()
{
    Console.WriteLine("Input the source wz file you want to use");
    string? sourceWzPath = Console.ReadLine();
    if (sourceWzPath == null)
    {
        Console.WriteLine("empty file path");
        return;
    }
    Console.WriteLine("Input the override wz file you want to use");
    string? overrideWzPath = Console.ReadLine();
    if (overrideWzPath == null)
    {
        Console.WriteLine("empty file path");
        return;
    }

    try
    {
        Merger merger = new Merger(sourceWzPath, overrideWzPath);
        Console.WriteLine("wz file loaded");
        Console.WriteLine("Input the path you want to merge");
        string? path = Console.ReadLine();
        if (path == null)
        {
            Console.WriteLine("empty path");
            return;
        }
        merger.Merge(path);
        Console.WriteLine("merge finished");
        Console.WriteLine("Input the path you want to save");
        string? savePath = Console.ReadLine();
        if (savePath == null)
        {
            Console.WriteLine("empty path");
            return;
        }
        merger.Save(savePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine("wz file merge failed :" + ex.Message);
        return;
    }
}


Run();
Console.WriteLine("Press any key...");
Console.ReadKey();

