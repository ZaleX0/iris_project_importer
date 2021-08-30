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
        public string[] GetPICFilePaths(string directoryPath)
        {
            string path = Path.GetDirectoryName(directoryPath);
            return Directory.GetFiles(path, "PIC_*.xml", SearchOption.AllDirectories);
        }

        public string GetIndexFilePath(string picFilePath)
        {
            try
            {
                DirectoryInfo dir = new FileInfo(picFilePath).Directory;
                return dir.Parent.Parent.Parent.GetFiles("Index.xml")[0].FullName;
            }
            catch
            {
                throw new Exception("Index.xml file missing.");
            }
        }
        
    }
}
