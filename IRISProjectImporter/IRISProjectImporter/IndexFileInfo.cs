using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRISProjectImporter
{
    class IndexFileInfo
    {
        public IndexFileInfo() { }

        public string vnk { get; set; }
        public string nnk { get; set; }
        public int von_stat { get; set; }
        public int bis_stat { get; set; }
        public string richtung { get; set; }
        public string cam { get; set; }
        public string datum { get; set; }
        public string version { get; set; }
        public string bemerkung { get; set; }
        public string volume { get; set; }
        public string picpath { get; set; }
        public int v_seither_km { get; set; }
        public int n_seither_km { get; set; }
        public int abs { get; set; }
        public string str_bez { get; set; }
        public int laenge { get; set; }
        public string kierunek { get; set; }
        public string nrodc { get; set; }
        public int km_lokp { get; set; }
        public int km_lokk { get; set; }
        public int km_globp { get; set; }
        public int km_globk { get; set; }
        public string phoml { get; set; }
    }
}
