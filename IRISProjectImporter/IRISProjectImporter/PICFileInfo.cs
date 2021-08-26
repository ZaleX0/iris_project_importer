using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRISProjectImporter
{
    class PICFileInfo
    {
        public PICFileInfo(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
        public string IndexXmlFilePath {
            get { return PICFileManager.GetIndexFile(Path); }
        }

    }
}
