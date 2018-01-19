using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mining_Station
{
    public partial class Chart : UserControl
    {
        public Chart()
        {
            InitializeComponent();
        }

        OxyPlot.Axes.Axis DateAxis;
        double DateAxisFullRange;
        double DateAxisDefaultMinimum;
        double DateAxisDefaultMaximum;

        OxyPlot.Axes.Axis PriceAxis;
        double PriceAxisFullRange;
        double PriceAxisDefaultMinimum;
        double PriceAxisDefaultMaximum;

        PlotModel ModelCache;

        private void PlotChart_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if(PlotChart.Model == null)
                PlotChart.Model = ModelCache;

            DateAxis = PlotChart.Model.Axes[0];
            PriceAxis = PlotChart.Model.Axes[1];

            // Emulates OxyPlot's internal logic of ActualMinimum / ActualMaximum calculation with padding.
            // Maximum needs to be calculated first since Minumum requires it.
            DateAxisDefaultMaximum = CalculateActualMaximum(DateAxis);
            DateAxisDefaultMinimum = CalculateActualMinimum(DateAxis, DateAxisDefaultMaximum);
            PriceAxisDefaultMaximum = CalculateActualMaximum(PriceAxis);
            PriceAxisDefaultMinimum = 0;

            DateAxisFullRange = DateAxisDefaultMaximum - DateAxisDefaultMinimum;
            PriceAxisFullRange = PriceAxisDefaultMaximum - PriceAxisDefaultMinimum;

            // Reassign key bindings.
            PlotChart.Controller = new PlotController();
            PlotChart.Controller.UnbindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Alt);
            PlotChart.Controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Control, OxyPlot.PlotCommands.PanAt);
        }

        // Readjust Plot's Minimum/Maximum based on sliders positions.
        private void RangeSliderHorizontal_RangeSelectionChanged(object sender, RangeSelectionChangedEventArgs e)
        {
            if (PlotChart.Model == null || DateAxis == null)
                return;

            var start = RangeSliderHorizontal.RangeStartSelected;
            var stop = RangeSliderHorizontal.RangeStopSelected;

            DateAxis.Minimum = DateAxisDefaultMinimum + (DateAxisFullRange * start / 100);
            DateAxis.Maximum = DateAxisDefaultMinimum + (DateAxisFullRange * stop / 100);
            PlotChart.InvalidatePlot(true);
        }

        // Readjust Plot's Minimum/Maximum based on sliders positions.
        private void RangeSliderVertical_RangeSelectionChanged(object sender, RangeSelectionChangedEventArgs e)
        {
            if (PlotChart.Model == null || PriceAxis == null)
                return;

            var start = RangeSliderVertical.RangeStartSelected;
            var stop = RangeSliderVertical.RangeStopSelected;

            PriceAxis.Minimum = PriceAxisDefaultMinimum + (PriceAxisFullRange * start / 100);
            PriceAxis.Maximum = PriceAxisDefaultMinimum + (PriceAxisFullRange * stop / 100);
            PlotChart.InvalidatePlot(true);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RangeSliderHorizontal.SetSelectedRange(0, 100);
            RangeSliderVertical.SetSelectedRange(0, 100);
            PlotChart.Model.ResetAllAxes();
            PlotChart.InvalidatePlot(true);
        }

        // The plot model needs to be nulled to avoid 'This PlotModel is already in use by some other PlotView' error.
        private void PlotChart_Unloaded(object sender, RoutedEventArgs e)
        {
            var plot = sender as OxyPlot.Wpf.PlotView;
            ModelCache = plot.Model;
            plot.Model = null;
        }

        // Oxyplot's internal logic. It is needed to calculate initial graph's padding on render.
        private double CalculateActualMinimum(OxyPlot.Axes.Axis axis, double actualMaximum)
        {
            var actualMinimum = axis.DataMinimum;
            double range = axis.DataMaximum - axis.DataMinimum;

            if (range < double.Epsilon)
            {
                double zeroRange = axis.DataMaximum > 0 ? axis.DataMaximum : 1;
                actualMinimum -= zeroRange * 0.5;
            }

            if (!double.IsNaN(actualMaximum))
            {
                double x1 = actualMaximum;
                double x0 = actualMinimum;
                double dx = axis.MinimumPadding * (x1 - x0);
                return x0 - dx;
            }

            return actualMinimum;
        }

        // Oxyplot's internal logic. It is needed to calculate initial graph's padding on render.
        private double CalculateActualMaximum(OxyPlot.Axes.Axis axis)
        {
            var actualMaximum = axis.DataMaximum;
            double range = axis.DataMaximum - axis.DataMinimum;

            if (range < double.Epsilon)
            {
                double zeroRange = axis.DataMaximum > 0 ? axis.DataMaximum : 1;
                actualMaximum += zeroRange * 0.5;
            }

            if (!double.IsNaN(axis.DataMinimum) && !double.IsNaN(actualMaximum))
            {
                double x1 = actualMaximum;
                double x0 = axis.DataMinimum;
                double dx = axis.MaximumPadding * (x1 - x0);
                return x1 + dx;
            }

            return actualMaximum;
        }
    }
}
