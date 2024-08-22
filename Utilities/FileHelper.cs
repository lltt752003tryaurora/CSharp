using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class FileHelper
    {
        private void LoadFileInDirectory(DirectoryInfo directory)
        {
            // Scan all files in the current path
            foreach (FileInfo file in directory.GetFiles())
            {
                // Do something with each file.
            }

            DirectoryInfo[] subDirectories = directory.GetDirectories();

            // Scan the directories in the current directory and call this method 
            // again to go one level into the directory tree
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                ScanDirectory(subDirectory);
            }
        }
        public static List<object> LoadListDirectory(string uploadPath, DirectoryInfo directory, string DS)
        {
            List<object> result = new List<object>();
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo di in subDirectories)
            {
                Dictionary<string, object> rs = new Dictionary<string, object>();
                rs.Add("data", di.Name);
                Dictionary<string, string> at1 = new Dictionary<string, string>();
                string subDirName = di.FullName.Substring(uploadPath.Length);
                at1.Add("directory", subDirName);
                rs.Add("attributes", at1);

                DirectoryInfo[] subDirectories1 = di.GetDirectories();
                if (subDirectories1.Length > 0)
                {
                    rs.Add("children", " ");
                }

                result.Add(rs);
            }
            return result;

            // Scan the directories in the current directory and call this method 
            // again to go one level into the directory tree
            //foreach (DirectoryInfo subDirectory in subDirectories)
            //{
            //    LoadListDirectory(subDirectory, result);
            //}
        }
        private void ScanDirectory(DirectoryInfo directory)
        {
            // Scan all files in the current path
            foreach (FileInfo file in directory.GetFiles())
            {
                // Do something with each file.
            }

            DirectoryInfo[] subDirectories = directory.GetDirectories();

            // Scan the directories in the current directory and call this method 
            // again to go one level into the directory tree
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                ScanDirectory(subDirectory);
            }
        }

        public static string GenerateFileName(string fileName)
        {
            var fileBase = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            return Common.MD5(time + fileBase) + ext;
        }
        public static void CreateFile(string path, string content)
        {

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();

                using (TextWriter tw = new StreamWriter(path))
                {
                    tw.WriteLine(content);
                }

            }
            else if (File.Exists(path))
            {
                using (TextWriter tw = new StreamWriter(path))
                {
                    tw.WriteLine(content);
                }
            }
        }
    }
}
