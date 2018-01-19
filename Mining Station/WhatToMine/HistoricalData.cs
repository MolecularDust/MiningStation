using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    [DataContract]
    public class HistoricalData
    {
        [BsonId][DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public Dictionary<string, WtmData> PriceData { get; set; }

        public HistoricalData() { }
        public HistoricalData(DateTime date, Dictionary<string, WtmData> wtmDataDict)
        {
            this.Date = date.RemoveTicks(); // Treats LiteDB DateTime idiosyncrasy
            this.PriceData = new Dictionary<string, WtmData>();
            foreach (var entry in wtmDataDict)
                this.PriceData.Add(entry.Key, entry.Value.Clone());
        }
        public HistoricalData Clone()
        {
            var _new = new HistoricalData();
            _new.Date = this.Date;
            _new.PriceData = new Dictionary<string, WtmData>();
            foreach (var entry in this.PriceData)
                _new.PriceData.Add(entry.Key, entry.Value.Clone());
            return _new;
        }
    }
}
