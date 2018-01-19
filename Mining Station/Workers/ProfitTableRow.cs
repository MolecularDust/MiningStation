using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Mining_Station
{
    public class ProfitTableRow : NotifyObject
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public string NameAndSymbol { get { return $"{this.Name} ({this.Symbol})"; }}
        [ScriptIgnore]
        [XmlIgnore]
        public bool Multicell { get; set; }
        public string Algorithm { get; set; }
        public string Hashrate { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public bool Switchable { get; set; }

        private bool _manualSwitch;
        [ScriptIgnore]
        [XmlIgnore]
        public bool ManualSwitch
        {
            get { return _manualSwitch; }
            set { _manualSwitch = value; OnPropertyChanged("ManualSwitch"); }
        }

        public string Path { get; set; }
        public string Arguments { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProfitDay { get; set; }
        public decimal ProfitWeek { get; set; }
        public decimal ProfitMonth { get; set; }
        public decimal ProfitYear { get; set; }
        public string Notes { get; set; }

        public ProfitTableRow() {}
    }
}
