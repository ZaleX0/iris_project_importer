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
            this.path = path;
        }
        private string path;
        public string IndexXmlFilePath {
            get { return new PICFileManager().GetIndexFilePath(path); }
        }

        public string pic_data_id { get; set; }
        public string index_data_id { get; set; }

        public string id_drogi { get; set; }
        public string vnk { get; set; }
        public string nnk { get; set; }
        public int abs { get; set; }
        public string version { get; set; }
        public string buchst { get; set; }
        public int station { get; set; }
        public int seiher_km { get; set; }
        public string filename { get; set; }
        public int format { get; set; }
        public string datum { get; set; }
        public double lat { get; set; }
        public string latns { get; set; }
        public double lon { get; set; }
        public string lonns { get; set; }
        public double alt { get; set; }
        public double heading { get; set; }
        public string picpath { get; set; }
        public double acc_lat { get; set; }
        public double acc_lon { get; set; }
        public double acc_alt { get; set; }
        public double acc_heading { get; set; }
        public double acc_roll { get; set; }
        public double acc_pitch { get; set; }
        public double roll { get; set; }
        public double pitch { get; set; }
        public double unix_time { get; set; }
        public double pic_id { get; set; }
    }
}
