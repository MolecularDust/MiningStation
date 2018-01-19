using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{

    public class CoinTableCopy
    {
        //What is being copied
        public CoinTable SourceCoinTable { get; set; }
        //Where from
        public Worker SourceWorker { get; set; }
        public int SourceCoinTableIndex { get; set; }
        //Where to
        public Worker DestinationWorker { get; set; }
        public int DestinationCoinTableIndex { get; set; }
    }
}
