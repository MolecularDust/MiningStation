using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mining_Station
{

    public class ValueToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal && (decimal)value < 0)
                return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThisPcToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                if ((string)parameter == "Border")
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    var brush = (Brush)converter.ConvertFromString("#FFF5FFF6");
                    return brush;
                }
                else return Brushes.Blue;
            else return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThisPcInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool thisPc = (bool)value;
            if (thisPc)
            {
                string coin = null;
                try
                {
                    coin = Shortcut.GetCurrentCoin().GetNameAndSymbol();
                    if (string.IsNullOrEmpty(coin))
                        return string.Empty;
                }
                catch 
                {
                    return string.Empty;
                }
                List<Inline> inlines = new List<Inline>();
                inlines.Add(new Run { Text = ", " });
                inlines.Add(new Run { Text = Environment.MachineName, FontWeight = FontWeights.SemiBold, Foreground = Brushes.Blue });
                inlines.Add(new Run { Text = $" is mining {coin}" });
                return inlines;
            }
            else return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SwitchToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                return "YES";
            else return "NO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToDaysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 1)
                return "day";
            else return "days";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class DaysHoursConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)values[0];
            int count = (int)values[1];
            if (count == 1)
                return name.TrimEnd('s').ToLower();
            else return name.ToLower();

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MultiBindingParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PeriodEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)values[0] && (bool)values[1])
                return true;
            else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if (value == null)
                return string.Empty;
            foreach (var entry in (ObservableCollection<string>)value)
                result = result + entry + ", ";
            return result.TrimEnd().TrimEnd(',');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new ObservableCollection<string>();
            var str = (string)value;
            if (str == string.Empty)
                return result;
            var list = str.Split(',');
            foreach (var enrty in list)
                result.Add(enrty.Trim());
            return result;
        }
    }

    public class MinutesCheckConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var fromHour = (int)values[0];
            var fromMinute = (int)values[1];
            var toHour = (int)values[2];
            if (toHour > fromHour)
                return (int)0;
            else return fromMinute;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class AddConditionParameterConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return values.ToArray();
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class SellButtonConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var balance = value as KeyValueEditablePair<string, PoloniexBalance>;
    //        if (balance == null || balance.Value.AvailableMarkets == null || balance.Value.AvailableMarkets.Count == 0 || balance.Value.Balance <= 0)
    //            return false;
    //        return true;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class BalanceConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if ((decimal)value == -1)
    //            return "???";
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}

    //public class BorderFirstConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var collection = values[0] as ObservableCollection<SellCondition>;
    //        if (collection == null)
    //            return false;
    //        var first = collection.FirstOrDefault();
    //        var condition = values[1] as SellCondition;
    //        if (first != null && first == condition)
    //            return true;
    //        else return false;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class ForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
                return Brushes.Red;
            else return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    public class CurrencyToStringFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyValue = (decimal)values[0];
            var currencyName = (string)values[1];
            var rate = (decimal)values[2];

            switch (currencyName)
            {
                case "USD":
                    return propertyValue.ToString("N2");
                case "BTC":
                    return (rate == 0) ? "?" : (propertyValue / rate).ToString("N4");
                default:
                    return (rate == 0) ? "?" : (propertyValue * rate).ToString("N4");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return (object[])value;
        }
    }

    public class ScheduledTimeConverted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dt = (DateTime)value;
            if (dt == default(DateTime))
                return " ---";
            else return dt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class RemoveLastSConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = (value as string) ;
            if (stringValue != null)
                return stringValue?.TrimEnd('s');
            else return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class OneOfTwoConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolOne = (bool)values[0];
            bool boolTwo = (bool)values[1];
            if (boolOne && !boolTwo)
                return true;
            else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BothAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolOne = (bool)values[0];
            bool boolTwo = (bool)values[1];
            if (boolOne && boolTwo)
                return true;
            else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class KillListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolOne = (bool)values[0];
            bool boolTwo = (bool)values[1];
            bool boolThree = (bool)values[2];
            if (boolOne && boolTwo && !boolThree)
                return true;
            else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ApplicationModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string role = (string)value;
            return role == "Standalone" ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class SelectedCoinConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var name = values[0] as string;
            var symbol = values[1] as string;
            var algorithm = values[2] as string;
            var status = values[3] as string;
            return new AlgoCoin { Name = name, Symbol = symbol, Algorithm = algorithm, Status = status};
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var coin = value as AlgoCoin;
            if (coin == null) return null;
            return new object[] { coin.Name, coin.Symbol, coin.Algorithm, coin.Status };
        }
    }

    //public class SelectedCoinColorConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string name = values[0] as string;
    //        List<AlgoCoin> coins = values[1] as List<AlgoCoin>;
    //        string status = 
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CoinStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            bool statusGood = text == null || text == string.Empty || string.Equals(text, "Active", StringComparison.InvariantCultureIgnoreCase);
            return statusGood ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
