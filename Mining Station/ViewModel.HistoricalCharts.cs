using LiteDB;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private void LoadHistoricalChartsCommand(object obj)
        {
            DateTime FromDate;
            DateTime ToDate;

            using (var db = new LiteDatabase(Constants.DataBase))
            {
                Helpers.MouseCursorWait();
                var collection = db.GetCollection<HistoricalData>(Constants.LightDB_HistoricalDataCollection);
                var allDays = collection.FindAll();
                if (allDays.Count() == 0)
                {
                    MessageBox.Show("The historical prices database is empty. Set up 'Accumulate Historical Prices', collect some stats for a while and return then.", "Nothing to show", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Helpers.MouseCursorNormal();
                    return;
                }
                var selectDatesWindow = new SelectDateRange();
                var viewModel = new SelectDateRangeVM(allDays.First().Date, allDays.Last().Date);
                selectDatesWindow.DataContext = viewModel;
                Helpers.MouseCursorNormal();
                var dialogResult = selectDatesWindow.ShowDialog();
                if (dialogResult == false)
                    return;
                FromDate = viewModel.FromDate;
                ToDate = viewModel.ToDate;
            }

            var workersChecked = Workers.WorkerList.Where(w => w.Query).ToList();
            var historicalCharts = new Dictionary<string, HistoricalChart>();

            // Create a chart for each worker
            foreach (var worker in workersChecked)
            {
                //var coinListFullName = Workers.GetCoins(new List<Worker> { worker }, false, false, true);
                var chart = new HistoricalChart {
                    Name = worker.Name,
                    Description = worker.Description,
                    Computers = new ObservableCollection<string>(worker.Computers),
                    Coins = new ObservablePairCollection<string, ChartCoin>(),
                    PlotModel = new PlotModel(),
                    DisplayCoinAs = Workers.DisplayCoinAs
                };
                chart.HookUpCoinsCollectionChanged();

                // Add AutoSwitch "coin"
                chart.Coins.Add("0, AutoSwitch", new ChartCoin { Color = OxyColors.Red.ToBrush(), IsChecked = true, Name = "AutoSwitch", Symbol = "AutoSwitch" });
                var serie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    Color = OxyColors.Red,
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = "AutoSwitch",
                    Smooth = false,
                    Tag = 0
                };
                chart.PlotModel.Series.Add(serie);


                // Add real coins
                int index = 1;
                foreach (var coinTable in worker.CoinList)
                {
                    var lockExpiry = FromDate.Date.Date.AddHours(WtmSettings.DelayNextSwitchTime);
                    var oxyColor = ChartColors.Colors[chart.PlotModel.Series.Count % ChartColors.Colors.Count];
                    chart.Coins.Add($"{index}, {coinTable.FullName}", new ChartCoin {
                        Index = index,
                        IsChecked = true,
                        LockedUntil = lockExpiry,
                        Color = oxyColor.ToBrush(),
                        Name = coinTable.FullName,
                        Symbol = coinTable.FullSymbol
                    });
                    var newSerie = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 3,
                        Color = oxyColor,
                        MarkerType = MarkerType.Circle,
                        CanTrackerInterpolatePoints = false,
                        Title = coinTable.FullName,
                        Smooth = false,
                        Tag = index
                    };
                    chart.PlotModel.Series.Add(newSerie);
                    index++;
                }

                CultureInfo currentCulture = CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.ToString());
                chart.PlotModel.Axes.Add(new DateTimeAxis
                {
                    IsPanEnabled = true,
                    IsZoomEnabled = false,
                    Position = AxisPosition.Bottom,
                    StringFormat = currentCulture.DateTimeFormat.ShortDatePattern,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    TextColor = ChartColors.DarkBlue,
                    TicklineColor = ChartColors.DarkBlue,
                    MajorGridlineColor = ChartColors.DarkBlue,
                    MinorGridlineThickness = 0.9,
                    AxislineThickness = 0.9,
                    MajorGridlineThickness = 0.9,
                    IntervalLength = 80
                });
                chart.PlotModel.Axes.Add(new LinearAxis
                {
                    IsZoomEnabled = false,
                    Position = AxisPosition.Left,
                    Title = "Daily profit",
                    Minimum = 0,
                    TitleColor = ChartColors.DarkBlue,
                    TextColor = ChartColors.DarkBlue,
                    TicklineColor = ChartColors.DarkBlue,
                    MajorGridlineColor = ChartColors.DarkBlue,
                    MinorTicklineColor = ChartColors.DarkBlue,
                    MinorGridlineColor = ChartColors.DarkBlue,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    AxislineThickness = 0.9,
                    MinorGridlineThickness = 0.9,
                    MajorGridlineThickness = 0.9,
                });

                chart.PlotModel.IsLegendVisible = false;
                chart.PlotModel.PlotAreaBorderThickness = new OxyThickness(0.9, 0.9, 0.9, 0.9);
                chart.PlotModel.PlotAreaBorderColor = ChartColors.DarkBlue;
                chart.PlotModel.Padding = new OxyThickness(0, 10, 0, -10);
                chart.PlotModel.DefaultColors = ChartColors.Colors;

                historicalCharts.Add(worker.Name, chart);
            }

            //Fill PlotModel with data
            using (var db = new LiteDatabase(Constants.DataBase))
            {
                var collection = db.GetCollection<HistoricalData>(Constants.LightDB_HistoricalDataCollection);
                var selectedDays = collection.Find(x => x.Date >= FromDate && x.Date <= ToDate);
                
                if (selectedDays.FirstOrDefault() == null)
                {
                    MessageBox.Show("The time selection is missing actual records in database.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DateTime previousDay = selectedDays.FirstOrDefault().Date.Date;

                foreach (var day in selectedDays)
                {
                    // Check if there's a gap and fill the missing days with blank data
                    var difference = (day.Date.Date - previousDay.Date).Days;
                    if (difference > 1)
                    {
                        for (int i = 1; i <= difference -1; i++)
                        {
                            foreach (var worker in workersChecked)
                            {
                                var index = 1;
                                var chart = historicalCharts[worker.Name]; // The chart that corresponds to the worker

                                chart.AutoSwitchCoinName = null;
                                var serie = chart.PlotModel.Series.First() as LineSeries;
                                var point = new DataPoint(DateTimeAxis.ToDouble(previousDay.Date.AddDays(i)), 0);
                                serie.Points.Add(point);

                                foreach (var coinTable in worker.CoinList)
                                {
                                    serie = chart.PlotModel.Series.FirstOrDefault(x => (int)x.Tag == index) as LineSeries;
                                    point = new DataPoint(DateTimeAxis.ToDouble(previousDay.Date.AddDays(i)), 0);
                                    serie.Points.Add(point);
                                    index++;
                                }
                            }
                        }
                    }

                    WtmData btc = null;
                    day.PriceData.TryGetValue("Bitcoin", out btc);

                    foreach (var worker in workersChecked)
                    {
                        var chart = historicalCharts[worker.Name]; // The chart that corresponds to the worker
                        var AutoSwitchPseudoCoin = chart.Coins["0, AutoSwitch"];

                        var index = 1;
                        foreach (var coinTable in worker.CoinList)
                        {
                            var currentChartCoin = chart.Coins[$"{index}, {coinTable.FullName}"];
                            if (currentChartCoin.Accumulator == null)
                                currentChartCoin.Accumulator = new Accumulator(coinTable.Coins);

                            // Calculate profit

                            var profitResult = WhatToMine.CalculateProfit(coinTable, day.PriceData, btc, WtmSettings, (decimal)Workers.PowerCost);
                            decimal profit24 = profitResult.profit24;

                            currentChartCoin.TodaysProfit = profit24;

                            // Accumulate daily reward
                            foreach (var coin in coinTable.Coins)
                            {

                                if (day.PriceData.ContainsKey(coin.Name))
                                {
                                    decimal ratio = (decimal)coin.Hashrate / (decimal)day.PriceData[coin.Name].DefaultHashrate;
                                    var dayReward = Helpers.StringToDecimal(day.PriceData[coin.Name].Json["estimated_rewards"]) * ratio; // Daily reward in mined coin
                                    dayReward -= dayReward * (decimal)coinTable.Fees / 100; // Deduct fees
                                    currentChartCoin.Accumulator.AddValue(coin.Name, dayReward);
                                }
                                else
                                {
                                    currentChartCoin.Accumulator.AddValue(coin.Name, 0);
                                }
                            }
                            currentChartCoin.PowerAccumulator += coinTable.Power / 1000 * 24; // Accumulate daily power consumption

                            // Sell accumulated reward if lock is expired
                            bool invalidateCoin = false;
                            if (day.Date.Date >= currentChartCoin.LockedUntil.Date)
                            {
                                decimal btcRate = 0;
                                if (WtmSettings.Average24)
                                    btcRate = btc != null ? Helpers.StringToDecimal(btc.Json["exchange_rate24"]) : 0;
                                else btcRate = btc != null ? Helpers.StringToDecimal(btc.Json["exchange_rate"]) : 0;
                                foreach (var coin in currentChartCoin.Accumulator.CoinList)
                                {
                                    if (day.PriceData.ContainsKey(coin.Name))
                                    {
                                        var coinRate = WtmSettings.Average24 ? 
                                            Helpers.StringToDecimal(day.PriceData[coin.Name].Json["exchange_rate24"]) :
                                            Helpers.StringToDecimal(day.PriceData[coin.Name].Json["exchange_rate"]);
                                        var revenue = coin.Value * coinRate * btcRate;
                                        currentChartCoin.TotalProfit += revenue;
                                        if (chart.AutoSwitchCoinName == coinTable.FullName)
                                            AutoSwitchPseudoCoin.TotalProfit += revenue;
                                    }
                                    else
                                    {
                                        invalidateCoin = true;
                                    }
                                }

                                if (!invalidateCoin)
                                {
                                    var powerExpense = (decimal)(currentChartCoin.PowerAccumulator * Workers.PowerCost);
                                    currentChartCoin.TotalProfit -= powerExpense;
                                    if (chart.AutoSwitchCoinName == coinTable.FullName)
                                        AutoSwitchPseudoCoin.TotalProfit -= powerExpense;
                                }
                                currentChartCoin.Accumulator.Clear();
                                currentChartCoin.PowerAccumulator = 0;
                                currentChartCoin.LockedUntil = day.Date.Date.AddHours(WtmSettings.DelayNextSwitchTime);
                            }

                            // Add a new point for this day to the Graph
                            var point = new DataPoint(DateTimeAxis.ToDouble(day.Date), (double)profit24);
                            var serie = chart.PlotModel.Series.FirstOrDefault(x => (int)x.Tag == index) as LineSeries;
                            if (serie != null)
                                serie.Points.Add(point);

                            index++;

                        }// foreach CoinTable

                        ChartCoin currentAutoSwitchCoin = null;

                        if (chart.AutoSwitchCoinName != null)
                            currentAutoSwitchCoin = chart.Coins[chart.AutoSwitchCoinName];
                            
                        // AutoSwitch emulator
                        if (day.Date.Date >= AutoSwitchPseudoCoin.LockedUntil.Date)
                        {
                            var mostProfitableCoin = chart.GetMostProfitableCoin();
                            decimal maxProfit = mostProfitableCoin.Value != null ? mostProfitableCoin.Value.TodaysProfit : 0;

                            if (chart.AutoSwitchCoinName == null)
                            {
                                chart.AutoSwitchCoinName = mostProfitableCoin.Key != null ? mostProfitableCoin.Key : null;
                                //AutoSwitchPseudoCoin.TotalProfit = mostProfitableCoin.Value != null ? mostProfitableCoin.Value.TotalProfit : 0;
                                currentAutoSwitchCoin = chart.AutoSwitchCoinName != null ? chart.Coins[chart.AutoSwitchCoinName] : null;
                            }

                            var currentProfit = chart.AutoSwitchCoinName != null ? chart.Coins[chart.AutoSwitchCoinName].TodaysProfit : 0;
                            if (maxProfit > (currentProfit + (currentProfit * WtmSettings.PriceMargin / 100))) // Switch coin in new profit is higher
                            {
                                chart.AutoSwitchCoinName = mostProfitableCoin.Key != null ? mostProfitableCoin.Key : null;
                                AutoSwitchPseudoCoin.LockedUntil = day.Date.Date.AddHours(WtmSettings.DelayNextSwitchTime);
                            }
                        }

                        var currentAutoSwitchCoinProfit = currentAutoSwitchCoin != null ? currentAutoSwitchCoin.TodaysProfit : 0;
                        AutoSwitchPseudoCoin.TotalProfit += currentAutoSwitchCoinProfit;
                        var autoSwitchPoint = new DataPoint(DateTimeAxis.ToDouble(day.Date), (double)currentAutoSwitchCoinProfit);
                        var autoSwitchSerie = chart.PlotModel.Series.First() as LineSeries;
                        autoSwitchSerie.Points.Add(autoSwitchPoint);
                    } // foreach Worker

                    previousDay = day.Date.Date;

                }// foreach days
            }

            HistoricalCharts.Clear();
            foreach (var chart in historicalCharts)
                HistoricalCharts.Add(chart.Value);

            HistoricalChartsEnabled = true;
            Helpers.MouseCursorWait();
        }

        private void LineSeriesSelectNoneCommand(object obj)
        {
            var chart = obj as HistoricalChart;
            if (chart == null)
                return;

            foreach (var coin in chart.Coins)
                coin.Value.IsChecked = false;
        }

        private void LineSeriesSelectAllCommand(object obj)
        {
            var chart = obj as HistoricalChart;
            if (chart == null)
                return;

            foreach (var coin in chart.Coins)
                coin.Value.IsChecked = true;
        }
    }
}
