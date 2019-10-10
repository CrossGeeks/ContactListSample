using System;
using System.IO;
using ContactListSample.Models;

namespace ContactListSample.Helpers
{
    public class FileHelper
    {
        public const string ThumbnailPrefix = "thumb";
        public const string TemporalDirectoryName = "TmpMedia";

        public static string GetUniquePath(string path, string name)
        {
            string ext = Path.GetExtension(name);
            if (ext == string.Empty)
                ext = ".jpg";

            name = Path.GetFileNameWithoutExtension(name);

            string nname = name + ext;
            int i = 1;
            while (File.Exists(Path.Combine(path, nname)))
                nname = name + "_" + (i++) + ext;

            return Path.Combine(path, nname);
        }


        public static string GetOutputPath(string path, string name)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);
            Directory.CreateDirectory(path);

            if (string.IsNullOrWhiteSpace(name))
            {
                string timestamp = DateTime.Now.ToString("yyyMMdd_HHmmss");
                name = "IMG_" + timestamp + ".jpg";
            }

            return Path.Combine(path, GetUniquePath(path, name));
        }
    }
}
