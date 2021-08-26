using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRISProjectImporter
{
    class PICFileManager
    {
        public static string[] GetPICFilePaths(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, "PIC_*.xml", SearchOption.AllDirectories);
        }

        public static string GetIndexFile(string picFilePath)
        {
            try
            {
                DirectoryInfo dir = new FileInfo(picFilePath).Directory;
                return dir.Parent.Parent.Parent.GetFiles("Index.xml")[0].FullName;
            }
            catch
            {
                throw new IndexOutOfRangeException("Index.xml file missing.");
            }
        }

        
    }
}
