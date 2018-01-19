using OxyPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mining_Station
{
    public static class Extensions
    {
        public static DateTime RemoveTicks(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, DateTimeKind.Local);
        }

        public static SolidColorBrush ToBrush(this OxyColor color)
        {
            var converter = new OxyPlot.Wpf.OxyColorConverter();
            var brush = converter.Convert(color, typeof(System.Windows.Media.Brush), null, null) as SolidColorBrush;
            return brush;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Helpers.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static System.Windows.Documents.Run FontWeight(this System.Windows.Documents.Run run, System.Windows.FontWeight fontWeight)
        {
            run.FontWeight = fontWeight;
            return run;
        }

        public static System.Windows.Documents.Run Color(this System.Windows.Documents.Run run, System.Windows.Media.Color color)
        {
            run.Foreground = new SolidColorBrush(color);
            return run;
        }

        public static char[] ToCharArray(this SecureString secureString)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException(nameof(secureString));
            }

            char[] charArray = new char[secureString.Length];
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                Marshal.Copy(unmanagedString, charArray, 0, charArray.Length);
                return charArray;
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeCoTaskMemUnicode(unmanagedString);
                }
            }
        }
    }
}
