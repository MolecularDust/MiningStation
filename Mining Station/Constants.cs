using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public static class Constants
    {
        public static string AppName = "Mining Station";
        public static char DecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        public const string WorkersFile = "Workers.cfg";
        public const string WtmSettingsFile = "WtmSettings.cfg";
        public const string SwitchLog = "switch.log";
        public const string HistoricalDataLog = "historical_data.log";
        public const string WtmLocalData = "WtmLocalData.data";

        public const string DataBase = "MiningStation.db";
        public const string LightDB_HistoricalDataCollection = "WtmHistoricalData";
        public const string LightDB_WtmCacheCollection = "WtmCache";
        public const string LightDB_TestCollection = "Test";

        public const string AccessPoint = "/MiningStation/AccessPoint";
        public const string StreamServer = "/MiningStation/StreamServer";

        public const string RunRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string BaseRegistryKey = "SOFTWARE\\Mining Station";
        public const string SwitchRegistryKey = "SOFTWARE\\Mining Station\\Switch";
        public const string UpdatePriceHistoryRegistryKey = "SOFTWARE\\Mining Station\\UpdatePriceHistory";
    }
}
