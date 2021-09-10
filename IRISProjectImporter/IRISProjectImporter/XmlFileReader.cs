using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IRISProjectImporter
{
    class XmlFileReader
    {
        public string GetElementContent(string xmlFilePath, string element)
        {
            XmlReader reader = XmlReader.Create(xmlFilePath);
            reader.ReadToFollowing(element);
            return reader.ReadElementContentAsString();
        }
        public string GetElementContent(string xmlFilePath, string element, params string[] moreElements)
        {
            XmlReader reader = XmlReader.Create(xmlFilePath);
            reader.ReadToFollowing(element);
            foreach (string item in moreElements)
            {
                reader.ReadToFollowing(item);
            }
            return reader.ReadElementContentAsString();
        }

        public IndexFileInfo GetIndexByPicPath(string picXmlPath)
        {
            DirectoryInfo picDir = new FileInfo(picXmlPath).Directory;
            DirectoryInfo indexDir = new FileInfo(picXmlPath).Directory.Parent.Parent.Parent;
            string indexPath = indexDir.GetFiles("Index.xml")[0].FullName;
            foreach (IndexFileInfo file in ReadAllIndexFileInfo(indexPath))
            {
                string indexPicPath = $"{picDir.Parent.Parent.Name}\\{picDir.Parent.Name}\\{picDir.Name}\\";
                if (file.picpath.Equals(indexPicPath))
                    return file;
            }
            return null;
        }



        public IEnumerable<IndexFileInfo> ReadAllIndexFileInfo(string xmlFilePath)
        {
            XmlReader r = XmlReader.Create(xmlFilePath);
            while (r.ReadToFollowing("VNK"))
            {
                IndexFileInfo index = new IndexFileInfo(xmlFilePath);
                index.vnk           = r.ReadElementContentAsString();   r.Read();
                index.nnk           = r.ReadElementContentAsString();   r.Read();
                index.von_stat      = r.ReadElementContentAsInt();      r.Read();
                index.bis_stat      = r.ReadElementContentAsInt();      r.Read();
                index.richtung      = r.ReadElementContentAsString();   r.Read();
                index.cam           = r.ReadElementContentAsString();   r.Read();
                index.datum         = r.ReadElementContentAsString();   r.Read();
                index.version       = r.ReadElementContentAsString();   r.Read();
                index.bemerkung     = r.ReadElementContentAsString();   r.Read();
                index.volume        = r.ReadElementContentAsString();   r.Read();
                index.picpath       = r.ReadElementContentAsString();   r.Read();
                index.v_seither_km  = r.ReadElementContentAsInt();      r.Read();
                index.n_seither_km  = r.ReadElementContentAsInt();      r.Read();
                index.abs           = r.ReadElementContentAsInt();      r.Read();
                index.str_bez       = r.ReadElementContentAsString();   r.Read();
                index.laenge        = r.ReadElementContentAsInt();      r.Read();
                index.kierunek      = r.ReadElementContentAsString();   r.Read();
                index.nrodc         = r.ReadElementContentAsString();   r.Read();
                index.km_lokp       = r.ReadElementContentAsInt();      r.Read();
                index.km_lokk       = r.ReadElementContentAsInt();      r.Read();
                index.km_globp      = r.ReadElementContentAsInt();      r.Read();
                index.km_globk      = r.ReadElementContentAsInt();      r.Read();
                index.phoml         = r.ReadElementContentAsString();   r.Read();
                yield return index;
            }
        }
        public IEnumerable<PICFileInfo> ReadAllPicFileInfo(string xmlFilePath)
        {
            XmlReader r = XmlReader.Create(xmlFilePath);
            while (r.ReadToFollowing("IDDROGI"))
            {
                PICFileInfo pic = new PICFileInfo(xmlFilePath);
                pic.id_drogi    = r.ReadElementContentAsString();  r.Read();
                pic.vnk         = r.ReadElementContentAsString();  r.Read();
                pic.nnk         = r.ReadElementContentAsString();  r.Read();
                pic.abs         = r.ReadElementContentAsInt();     r.Read();
                pic.version     = r.ReadElementContentAsString();  r.Read();
                pic.buchst      = r.ReadElementContentAsString();  r.Read();
                pic.station     = r.ReadElementContentAsInt();     r.Read();
                pic.seiher_km   = r.ReadElementContentAsInt();     r.Read();
                pic.filename    = r.ReadElementContentAsString();  r.Read();
                pic.format      = r.ReadElementContentAsInt();     r.Read();
                pic.datum       = r.ReadElementContentAsString();  r.Read();
                pic.lat         = r.ReadElementContentAsDouble();  r.Read();
                pic.latns       = r.ReadElementContentAsString();  r.Read();
                pic.lon         = r.ReadElementContentAsDouble();  r.Read();
                pic.lonew       = r.ReadElementContentAsString();  r.Read();
                pic.alt         = r.ReadElementContentAsDouble();  r.Read();
                pic.heading     = r.ReadElementContentAsDouble();  r.Read();
                pic.picpath     = r.ReadElementContentAsString();  r.Read();
                pic.acc_lat     = r.ReadElementContentAsDouble();  r.Read();
                pic.acc_lon     = r.ReadElementContentAsDouble();  r.Read();
                pic.acc_alt     = r.ReadElementContentAsDouble();  r.Read();
                pic.acc_heading = r.ReadElementContentAsDouble();  r.Read();
                pic.acc_roll    = r.ReadElementContentAsDouble();  r.Read();
                pic.acc_pitch   = r.ReadElementContentAsDouble();  r.Read();
                pic.roll        = r.ReadElementContentAsDouble();  r.Read();
                pic.pitch       = r.ReadElementContentAsDouble();  r.Read();
                pic.unix_time   = r.ReadElementContentAsDouble();  r.Read();
                pic.pic_id      = r.ReadElementContentAsDouble();  r.Read();
                yield return pic;
            }
        }



        public IndexFileInfo[] IndexFileInfoArray(string xmlFilePath)
        {
            XmlReader r = XmlReader.Create(xmlFilePath);
            List<IndexFileInfo> list = new List<IndexFileInfo>();
            while (r.ReadToFollowing("VNK"))
            {
                IndexFileInfo index = new IndexFileInfo(xmlFilePath);
                index.vnk = r.ReadElementContentAsString(); r.Read();
                index.nnk = r.ReadElementContentAsString(); r.Read();
                index.von_stat = r.ReadElementContentAsInt(); r.Read();
                index.bis_stat = r.ReadElementContentAsInt(); r.Read();
                index.richtung = r.ReadElementContentAsString(); r.Read();
                index.cam = r.ReadElementContentAsString(); r.Read();
                index.datum = r.ReadElementContentAsString(); r.Read();
                index.version = r.ReadElementContentAsString(); r.Read();
                index.bemerkung = r.ReadElementContentAsString(); r.Read();
                index.volume = r.ReadElementContentAsString(); r.Read();
                index.picpath = r.ReadElementContentAsString(); r.Read();
                index.v_seither_km = r.ReadElementContentAsInt(); r.Read();
                index.n_seither_km = r.ReadElementContentAsInt(); r.Read();
                index.abs = r.ReadElementContentAsInt(); r.Read();
                index.str_bez = r.ReadElementContentAsString(); r.Read();
                index.laenge = r.ReadElementContentAsInt(); r.Read();
                index.kierunek = r.ReadElementContentAsString(); r.Read();
                index.nrodc = r.ReadElementContentAsString(); r.Read();
                index.km_lokp = r.ReadElementContentAsInt(); r.Read();
                index.km_lokk = r.ReadElementContentAsInt(); r.Read();
                index.km_globp = r.ReadElementContentAsInt(); r.Read();
                index.km_globk = r.ReadElementContentAsInt(); r.Read();
                index.phoml = r.ReadElementContentAsString(); r.Read();
                list.Add(index);
            }
            return list.ToArray();
        }
        public PICFileInfo[] PicFileInfoArray(string xmlFilePath)
        {
            XmlReader r = XmlReader.Create(xmlFilePath);
            List<PICFileInfo> list = new List<PICFileInfo>();
            while (r.ReadToFollowing("IDDROGI"))
            {
                PICFileInfo pic = new PICFileInfo(xmlFilePath);
                pic.id_drogi = r.ReadElementContentAsString(); r.Read();
                pic.vnk = r.ReadElementContentAsString(); r.Read();
                pic.nnk = r.ReadElementContentAsString(); r.Read();
                pic.abs = r.ReadElementContentAsInt(); r.Read();
                pic.version = r.ReadElementContentAsString(); r.Read();
                pic.buchst = r.ReadElementContentAsString(); r.Read();
                pic.station = r.ReadElementContentAsInt(); r.Read();
                pic.seiher_km = r.ReadElementContentAsInt(); r.Read();
                pic.filename = r.ReadElementContentAsString(); r.Read();
                pic.format = r.ReadElementContentAsInt(); r.Read();
                pic.datum = r.ReadElementContentAsString(); r.Read();
                pic.lat = r.ReadElementContentAsDouble(); r.Read();
                pic.latns = r.ReadElementContentAsString(); r.Read();
                pic.lon = r.ReadElementContentAsDouble(); r.Read();
                pic.lonew = r.ReadElementContentAsString(); r.Read();
                pic.alt = r.ReadElementContentAsDouble(); r.Read();
                pic.heading = r.ReadElementContentAsDouble(); r.Read();
                pic.picpath = r.ReadElementContentAsString(); r.Read();
                pic.acc_lat = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_lon = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_alt = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_heading = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_roll = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_pitch = r.ReadElementContentAsDouble(); r.Read();
                pic.roll = r.ReadElementContentAsDouble(); r.Read();
                pic.pitch = r.ReadElementContentAsDouble(); r.Read();
                pic.unix_time = r.ReadElementContentAsDouble(); r.Read();
                pic.pic_id = r.ReadElementContentAsDouble(); r.Read();
                list.Add(pic);
            }
            return list.ToArray();
        }
        public PICFileInfo[] PicFileInfoArray_buchst_XorP(string xmlFilePath)
        {
            XmlReader r = XmlReader.Create(xmlFilePath);
            List<PICFileInfo> list = new List<PICFileInfo>();
            while (r.ReadToFollowing("IDDROGI"))
            {
                PICFileInfo pic = new PICFileInfo(xmlFilePath);
                pic.id_drogi = r.ReadElementContentAsString(); r.Read();
                pic.vnk = r.ReadElementContentAsString(); r.Read();
                pic.nnk = r.ReadElementContentAsString(); r.Read();
                pic.abs = r.ReadElementContentAsInt(); r.Read();
                pic.version = r.ReadElementContentAsString(); r.Read();
                pic.buchst = r.ReadElementContentAsString(); r.Read();
                pic.station = r.ReadElementContentAsInt(); r.Read();
                pic.seiher_km = r.ReadElementContentAsInt(); r.Read();
                pic.filename = r.ReadElementContentAsString(); r.Read();
                pic.format = r.ReadElementContentAsInt(); r.Read();
                pic.datum = r.ReadElementContentAsString(); r.Read();
                pic.lat = r.ReadElementContentAsDouble(); r.Read();
                pic.latns = r.ReadElementContentAsString(); r.Read();
                pic.lon = r.ReadElementContentAsDouble(); r.Read();
                pic.lonew = r.ReadElementContentAsString(); r.Read();
                pic.alt = r.ReadElementContentAsDouble(); r.Read();
                pic.heading = r.ReadElementContentAsDouble(); r.Read();
                pic.picpath = r.ReadElementContentAsString(); r.Read();
                pic.acc_lat = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_lon = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_alt = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_heading = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_roll = r.ReadElementContentAsDouble(); r.Read();
                pic.acc_pitch = r.ReadElementContentAsDouble(); r.Read();
                pic.roll = r.ReadElementContentAsDouble(); r.Read();
                pic.pitch = r.ReadElementContentAsDouble(); r.Read();
                pic.unix_time = r.ReadElementContentAsDouble(); r.Read();
                pic.pic_id = r.ReadElementContentAsDouble(); r.Read();

                if (pic.buchst.Equals("X") || pic.buchst.Equals("P"))
                    list.Add(pic);
            }
            return list.ToArray();
        }
    }
}
