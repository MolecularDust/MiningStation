using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    [DataContract]
    public class WtmData
    {
        [DataMember]
        public double DefaultHashrate { get; set; }
        [DataMember]
        public Dictionary<string, string> Json { get; set; }
        [ScriptIgnore][DataMember]
        public bool HasAverage { get; set; } = false;
        [ScriptIgnore][DataMember]
        public int AverageDays { get; set; } = 0;
        [ScriptIgnore][DataMember]
        public decimal AveragePrice { get; set; } = 0;

        public WtmData() {}

        public WtmData Clone()
        {
            var _new = new WtmData();
            //_new.Coin = this.Coin;
            _new.DefaultHashrate = this.DefaultHashrate;
            _new.Json = new Dictionary<string, string>();
            foreach (var entry in this.Json)
                _new.Json.Add(entry.Key, entry.Value);
            _new.HasAverage = this.HasAverage;
            _new.AverageDays = this.AverageDays;
            _new.AveragePrice = this.AveragePrice;
            return _new;
        }
    }
}
