using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRISProjectImporter
{
    class IndexFileManager
    {
        public string[] GetIndexFilePaths(string[] picFilePaths)
        {
            HashSet<string> pathSet = new HashSet<string>();
            PICFileManager picManager = new PICFileManager();
            for (int i = 0; i < picFilePaths.Length; i++)
            {
                pathSet.Add(picManager.GetIndexFilePath(picFilePaths[i]));
            }
            return pathSet.ToArray();
        }
    }
}
