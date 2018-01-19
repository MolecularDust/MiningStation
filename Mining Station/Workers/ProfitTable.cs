using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Mining_Station
{
    public class ProfitTable : NotifyObject
    {
        public string Name { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public int Index { get; set; }

        private ObservableCollection<Computer> _computers;
        [ScriptIgnore]
        [XmlIgnore]
        public ObservableCollection<Computer> Computers
        {
            get { return _computers; }
            set { _computers = value; }
        }
      
        public List<string> ComputerNames { get { return this.Computers.Select(x => x.Name).ToList(); } }

        [ScriptIgnore]
        [XmlIgnore]
        public bool ThisPC { get; set; }
        public string Description { get; set; }
        public List<ProfitTableRow> ProfitList { get; set; }

        public ProfitTable()
        {
        }

        public void HookPropertyChanched()
        {
            if (ProfitList == null)
                return;
            foreach (var row in ProfitList)
                //PropertyChangedEventManager.AddHandler(row, Row_PropertyChanged, "");
                row.PropertyChanged += Row_PropertyChanged;
        }

        public void Row_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var row = sender as ProfitTableRow;
            if (row == null)
                return;
            if (e.PropertyName == "ManualSwitch" && row != null && row.ManualSwitch == true)
            {
                foreach (var computer in Computers)
                {
                    computer.NewCoinName = row.Name;
                    computer.NewCoinSymbol = row.Symbol;
                    computer.RaiseProperychanged("NewCoinNameAndSymbol");
                }
            }
        }
    }
}
