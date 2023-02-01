using System;
using System.Text.RegularExpressions;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace WzMerger
{
    public class Merger
    {
        string baseDir;
        string overrideDir;

        public Merger(string baseDir, string overrideDir)
        {
            this.baseDir = baseDir;
            this.overrideDir = overrideDir;
        }

        private WzFile loadWzFile(string wzPath)
        {
            short fileVersion = 0;
            var gameVersion = WzTool.DetectMapleVersion(wzPath, out fileVersion);
            WzFile f = new WzFile(wzPath, fileVersion, gameVersion);
            var parseResult = f.ParseWzFile();
            if (parseResult == WzFileParseStatus.Success)
            {
                return f;
            }
            else
            {
                throw new Exception(WzFileParseStatusExtension.GetErrorDescription(parseResult));
            }

        }

        public void Merge(string path, string savePath)
        {
            var paths = File.ReadAllLines(path);
            var wzFileDict = new Dictionary<string, List<string>>();
            foreach (var p in paths)
            {
                var split = p.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (!wzFileDict.ContainsKey(split[0]))
                {
                    wzFileDict.Add(split[0], new List<string>());
                }
                wzFileDict[split[0]].Add(p);
            }
            foreach (var pair in wzFileDict)
            {
                Console.WriteLine("start to merge " + pair.Key + " ...");
                var baseFile = loadWzFile(Path.Join(this.baseDir, pair.Key));
                var overrideFile = loadWzFile(Path.Join(this.overrideDir, pair.Key));
                var pathsToMerge = pair.Value;
                foreach (var pathToMerge in pathsToMerge)
                {
                    if (!pathCheck(pathToMerge, baseFile, overrideFile))
                    {
                        Console.WriteLine("path " + pathToMerge + " not found skip");
                        continue;
                    }
                    mergePath(pathToMerge, baseFile, overrideFile);
                }

                Console.WriteLine("merge finished, saving to disk ...");
                baseFile.SaveToDisk(Path.Join(savePath, pair.Key), false, baseFile.MapleVersion);
            }
        }

        bool pathCheck(string path, WzFile baseFile, WzFile overrideFile)
        {
            var baseObj = baseFile.GetObjectFromPath(path, true);
            if (baseObj == null)
            {
                return false;
            }
            var overrideObj = overrideFile.GetObjectFromPath(path, true);
            if (overrideObj == null)
            {
                return false;
            }

            return true;
        }

        void mergePath(string path, WzFile baseFile, WzFile overrideFile)
        {
            var baseObj = baseFile.GetObjectFromPath(path, true);
            var overrideObj = overrideFile.GetObjectFromPath(path, true);


            replaceObject(baseObj, overrideObj);
        }


        void replaceObject(WzObject wz, WzObject wz1)
        {
            if (wz.ObjectType != wz1.ObjectType)
            {
                throw new Exception("needed merge file had different type");
            }
            if (compareObject(wz, wz1))
            {
                Console.WriteLine("same object, skip");
                return;
            }
            switch (wz.ObjectType)
            {
                case WzObjectType.Directory:
                    replaceDirectory((WzDirectory)wz, (WzDirectory)wz1);
                    break;
                case WzObjectType.Image:
                    replaceImage((WzImage)wz, (WzImage)wz1);
                    break;
                case WzObjectType.Property:
                    replaceProperty((WzImageProperty)wz, (WzImageProperty)wz1);
                    break;
                default:
                    throw new Exception("not supported");
            }
        }

        bool compareObject(WzObject wz, WzObject wz1)
        {
            if (wz.ObjectType != wz1.ObjectType)
            {
                throw new Exception("needed merge file had different type");
            }
            switch (wz.ObjectType)
            {
                case WzObjectType.Directory:
                    return compareDirectory((WzDirectory)wz, (WzDirectory)wz1);
                case WzObjectType.Image:
                    return compareImage((WzImage)wz, (WzImage)wz1);
                case WzObjectType.Property:
                    return compareProperty((WzImageProperty)wz, (WzImageProperty)wz1);
                default:
                    throw new Exception("not supported");
            }
        }

        void replaceDirectory(WzDirectory dir1, WzDirectory dir2)
        {
            if (dir1.Name != dir2.Name)
            {
                throw new Exception("needed merge file had different name");
            }
            var subDirs1 = dir1.WzDirectories.ToDictionary(x => x.Name);
            var subDirs2 = dir2.WzDirectories.ToDictionary(x => x.Name);
            foreach (var key in subDirs1.Keys)
            {
                if (!subDirs2.ContainsKey(key) || compareDirectory(subDirs1[key], subDirs2[key]))
                {
                    continue;
                }
                replaceDirectory(subDirs1[key], subDirs2[key]);
            }
            foreach (var key in subDirs2.Keys)
            {
                if (!subDirs1.ContainsKey(key))
                {
                    dir1.AddDirectory(subDirs2[key]);
                }
            }

            var subImgs1 = dir1.WzImages.ToDictionary(x => x.Name);
            var subImgs2 = dir2.WzImages.ToDictionary(x => x.Name);
            foreach (var key in subImgs1.Keys)
            {
                if (!subImgs2.ContainsKey(key) || compareImage(subImgs1[key], subImgs2[key]))
                {
                    continue;
                }
                replaceImage(subImgs1[key], subImgs2[key]);
            }
            foreach (var key in subImgs2.Keys)
            {
                if (!subImgs1.ContainsKey(key))
                {
                    dir1.AddImage(subImgs2[key]);
                }
            }
        }

        bool compareDirectory(WzDirectory dir1, WzDirectory dir2)
        {
            if (dir1.Name != dir2.Name)
            {
                return false;
            }
            if (dir1.WzDirectories.Count != dir2.WzDirectories.Count)
            {
                return false;
            }
            var subDirs1 = dir1.WzDirectories.ToDictionary(x => x.Name);
            var subDirs2 = dir2.WzDirectories.ToDictionary(x => x.Name);
            foreach (var key in subDirs1.Keys)
            {
                if (!subDirs2.ContainsKey(key))
                {
                    return false;
                }
                if (!compareDirectory(subDirs1[key], subDirs2[key]))
                {
                    return false;
                }
            }

            if (dir1.WzImages.Count != dir2.WzImages.Count)
            {
                return false;
            }
            var subImgs1 = dir1.WzImages.ToDictionary(x => x.Name);
            var subImgs2 = dir2.WzImages.ToDictionary(x => x.Name);
            foreach (var key in subImgs1.Keys)
            {
                if (!subImgs2.ContainsKey(key))
                {
                    return false;
                }
                if (!compareImage(subImgs1[key], subImgs2[key]))
                {
                    return false;
                }
            }

            return true;
        }

        bool compareImage(WzImage img1, WzImage imgs)
        {
            if (img1.Name != imgs.Name)
            {
                return false;
            }
            if (img1.WzProperties.Count != imgs.WzProperties.Count)
            {
                return false;
            }
            var dict1 = img1.WzProperties.ToDictionary(x => x.Name);
            var dict2 = imgs.WzProperties.ToDictionary(x => x.Name);
            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
                if (!compareProperty(dict1[key], dict2[key]))
                {
                    return false;
                }
            }

            return true;
        }

        void replaceImage(WzImage img1, WzImage imgs)
        {
            if (img1.Name != imgs.Name)
            {
                throw new Exception("name not match");
            }
            img1.Changed = true;
            var dict1 = img1.WzProperties.ToDictionary(x => x.Name);
            var dict2 = imgs.WzProperties.ToDictionary(x => x.Name);
            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key) || compareProperty(dict1[key], dict2[key]))
                {
                    continue;
                }
                replaceProperty(dict1[key], dict2[key]);
            }
            foreach (var key in dict2.Keys)
            {
                if (!dict1.ContainsKey(key))
                {
                    img1.AddProperty(dict2[key]);
                }
            }
        }

        bool compareProperty(WzImageProperty prop1, WzImageProperty prop2)
        {
            if (prop1.Name != prop2.Name)
            {
                return false;
            }
            if (prop1.PropertyType != prop2.PropertyType)
            {
                return false;
            }

            if (prop1.PropertyType == WzPropertyType.SubProperty)
            {
                var subP1 = (WzSubProperty)prop1;
                var subP2 = (WzSubProperty)prop2;
                if (subP1.WzProperties.Count != subP2.WzProperties.Count)
                {
                    return false;
                }
                var dict1 = subP1.WzProperties.ToDictionary(x => x.Name);
                var dict2 = subP2.WzProperties.ToDictionary(x => x.Name);
                foreach (var key in dict1.Keys)
                {
                    if (!dict2.ContainsKey(key))
                    {
                        return false;
                    }
                    if (!compareProperty(dict1[key], dict2[key]))
                    {
                        return false;
                    }
                }
            }
            else if (prop1.PropertyType == WzPropertyType.String)
            {
                return prop1.WzValue == prop2.WzValue;
            }

            return true;
        }

        void replaceProperty(WzImageProperty prop1, WzImageProperty prop2)
        {
            if (prop1.Name != prop2.Name)
            {
                throw new Exception("name not match");
            }
            if (prop1.PropertyType != prop2.PropertyType)
            {
                Console.WriteLine("prop type not match {0} {1} {2}", prop1.Name, prop1.PropertyType, prop2.PropertyType);
                return;
                // throw new Exception("type not match");
            }
            if (prop1.PropertyType == WzPropertyType.SubProperty)
            {
                var subP1 = (WzSubProperty)prop1;
                var subP2 = (WzSubProperty)prop2;
                var dict1 = subP1.WzProperties.ToDictionary(x => x.Name);
                var dict2 = subP2.WzProperties.ToDictionary(x => x.Name);
                foreach (var key in dict1.Keys)
                {
                    if (!dict2.ContainsKey(key) || compareProperty(dict1[key], dict2[key]))
                    {
                        continue;
                    }
                    replaceProperty(dict1[key], dict2[key]);
                }
                foreach (var key in dict2.Keys)
                {
                    if (!dict1.ContainsKey(key))
                    {
                        subP1.AddProperty(dict2[key]);
                    }
                }
            }
            if (prop1.PropertyType != WzPropertyType.String)
            {
                return;
            }
            if (prop1.WzValue != prop2.WzValue && prop2.GetString().Any(x => Util.IsChinese(x)))
            {
                prop1.SetValue(prop2.WzValue);

            }
        }


    }

    public static class Util
    {
        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        public static bool IsChinese(this char c)
        {
            return cjkCharRegex.IsMatch(c.ToString());
        }
    }
}

