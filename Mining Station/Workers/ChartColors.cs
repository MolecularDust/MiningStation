using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ChartColors
    {
        public static List<OxyColor> Colors = new List<OxyColor>
        {
            OxyColor.FromUInt32(0xff4e9a06),
            OxyColor.FromUInt32(0xffc88d00),
            OxyColor.FromUInt32(0xffcc0000),
            OxyColor.FromUInt32(0xff204a87),
            OxyColor.FromUInt32(0xffffa500),
            OxyColor.FromUInt32(0xff008000),
            OxyColor.FromUInt32(0xff0000ff),
            OxyColor.FromUInt32(0xff4b0082),
            OxyColor.FromUInt32(0xFFFF6347),
            OxyColor.FromUInt32(0xFF800080),
            OxyColor.FromUInt32(0xFFBC8F8F),
            OxyColor.FromUInt32(0xFF4169E1),
            OxyColor.FromUInt32(0xFF8B4513),
            OxyColor.FromUInt32(0xFFFA8072),
            OxyColor.FromUInt32(0xFFF4A460),
            OxyColor.FromUInt32(0xFF2E8B57),
            OxyColor.FromUInt32(0xffa6cee3),
            OxyColor.FromUInt32(0xff1f78b4),
            OxyColor.FromUInt32(0xffb2df8a),
            OxyColor.FromUInt32(0xff33a02c),
            OxyColor.FromUInt32(0xfffb9a99),
            OxyColor.FromUInt32(0xffe31a1c),
            OxyColor.FromUInt32(0xfffdbf6f),
            OxyColor.FromUInt32(0xffff7f00),
            OxyColor.FromUInt32(0xffcab2d6),
            OxyColor.FromUInt32(0xff6a3d9a),
            OxyColor.FromUInt32(0xffffff99),
            OxyColor.FromUInt32(0xffb15928),
        };

        public static OxyColor DarkBlue = OxyColor.Parse("#688CAF");

        public static List<OxyColor> RandomColors { get; set; }

        public static OxyColor GetNextColor(ref int index)
        {
            if (index + 1 >= Colors.Count)
                index = 0;
            else index++;
            return Colors[index];
        }

        public static OxyColor GetRandomColor()
        {
            return Colors[Helpers.Random.Next(Colors.Count)];
        }
    }
}
