using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class AccumulatedCoin
    {
        public string Name { get; set; }
        public decimal Value { get; set; }

        public AccumulatedCoin(string coinName, decimal value)
        {
            this.Name = coinName;
            this.Value = value;
        }
    }

    public class Accumulator
    {
        public List<AccumulatedCoin> CoinList { get; set; } = new List<AccumulatedCoin>();

        public Accumulator() { }

        public Accumulator(IList<Coin> list)
        {
            foreach (var coin in list)
                CoinList.Add(new AccumulatedCoin(coin.Name, 0M));
        }

        public void Clear()
        {
            CoinList.Clear();
        }

        public void AddValue(string coin, decimal value)
        {
            var foundCoin = CoinList.FirstOrDefault(x => x.Name == coin);
            if (foundCoin == null)
                CoinList.Add(new AccumulatedCoin(coin, value));
            else foundCoin.Value += value;
        }

        public decimal? GetValue(string coin)
        {
            var foundCoin = CoinList.FirstOrDefault(x => x.Name == coin);
            return foundCoin.Value;
        }

    }
}
