using System;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace WzMerger
{
    public class Merger
    {
        WzFile baseFile;
        WzFile overrideFile;

        public Merger(string baseWz, string overrideWz)
        {
            this.baseFile = loadWzFile(baseWz);
            this.overrideFile = loadWzFile(overrideWz);
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

        public void Merge(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == ".txt")
            {
                var paths = File.ReadAllLines(path);
                foreach (var p in paths)
                {
                    if (!pathCheck(path))
                    {
                        Console.WriteLine("path [" + p + "] not valid in selected wz file,skip");
                        continue;
                    }
                    mergePath(p);
                }
            }
            else
            {
                if (!pathCheck(path))
                {
                    throw new Exception("path not valid in selected wz file");
                }
                mergePath(path);
            }
        }

        bool pathCheck(string path)
        {
            var baseObj = this.baseFile.GetObjectFromPath(path, true);
            if (baseObj == null)
            {
                return false;
            }
            var overrideObj = this.overrideFile.GetObjectFromPath(path, true);
            if (overrideObj == null)
            {
                return false;
            }

            return true;
        }

        void mergePath(string path)
        {
            var baseObj = this.baseFile.GetObjectFromPath(path, true);
            var overrideObj = this.overrideFile.GetObjectFromPath(path, true);


            replaceObject(baseObj, overrideObj);
        }

        public void Save(string path)
            
        {
            this.baseFile.SaveToDisk(path, false, this.baseFile.MapleVersion);
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
            if (img1.WzProperties.Count != imgs.WzProperties.Count)
            {
                throw new Exception("property count not match");
            }
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
                throw new Exception("type not match");
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
            if (prop1.WzValue != prop2.WzValue)
            {
                prop1.SetValue(prop2.WzValue);
            }
        }
    }


}

