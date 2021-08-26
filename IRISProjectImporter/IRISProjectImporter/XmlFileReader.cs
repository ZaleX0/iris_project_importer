using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IRISProjectImporter
{
    class XmlFileReader
    {
        public static string GetElementContent(string xmlFilePath, string element, params string[] moreElements)
        {
            XmlReader reader = XmlReader.Create(xmlFilePath);
            reader.ReadToFollowing(element);
            foreach (string item in moreElements)
            {
                reader.ReadToFollowing(item);
            }
            return reader.ReadElementContentAsString();
        }

    }
}
