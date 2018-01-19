using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mining_Station
{
    public class Helpers
    {
        public static readonly Random Random = new Random();

        public static void HookUpPropertyChanged<T>(NotifyCollectionChangedEventArgs e, PropertyChangedEventHandler handler)
            where T : INotifyPropertyChanged
        {
            if (e.NewItems != null)
                foreach (T item in e.NewItems)
                    item.PropertyChanged += handler;
            //PropertyChangedEventManager.AddHandler(item, handler, "");

            if (e.OldItems != null)
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= handler;
            //PropertyChangedEventManager.RemoveHandler(item, handler, "");
        }


        public static void HookUpPropertyChanging<T>(NotifyCollectionChangedEventArgs e, PropertyChangingEventHandler handler)
            where T : INotifyPropertyChanging
        {
            if (e.NewItems != null)
                foreach (T item in e.NewItems)
                    item.PropertyChanging += handler;
            //PropertyChangingEventManager.AddHandler(item, handler);

            if (e.OldItems != null)
                foreach (T item in e.OldItems)
                    item.PropertyChanging -= handler;
            //PropertyChangingEventManager.RemoveHandler(item, handler);
        }


        public static void ShowErrorMessage(string msg, string title = "Error")
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                MessageBox.Show(Application.Current.MainWindow, msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    Application.Current.MainWindow.Activate();
                    MessageBox.Show(Application.Current.MainWindow, msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

        public static int Fibonacci(int n)
        {
            return n > 1 ? Fibonacci(n - 1) + Fibonacci(n - 2) : n;
        }

        public static void WriteToTxtFile(string fileName, string content)
        {
            try
            {
                System.IO.File.WriteAllText(fileName, content);
            }
            catch (Exception e)
            {

                ShowErrorMessage(e.Message);
            }
        }

        public static DependencyObject FindAncestor(DependencyObject current, Type type, int levels)
        {
            int currentLevel = 0;
            while (current != null)
            {
                if (current.GetType() == type)
                {
                    currentLevel++;
                    if (currentLevel == levels)
                    {
                        return current;
                    }
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }


        public static DependencyObject FindAncestorWithExclusion(DependencyObject obj, Type type, int maxMatchLevels, List<Type> noGoTypes)
        {
            int currentLevel = 0;
            while (obj != null)
            {
                var currentType = obj.GetType();
                if (noGoTypes.Contains(currentType))
                    return null;
                if (currentType == type)
                {
                    currentLevel++;
                    if (currentLevel <= maxMatchLevels)
                        return obj;
                    else return null;
                }
                obj = VisualTreeHelper.GetParent(obj);
            };
            return null;
        }

        public static DependencyObject FindAncestorByDataContext(DependencyObject obj, Type type, int maxMatchLevels, Type dataContextType, List<Type> noGoTypes)
        {
            int currentLevel = 0;
            while (obj != null)
            {
                var currentType = obj.GetType();
                if (noGoTypes.Contains(currentType))
                    return null;
                if (currentType == type && ((Control)obj).DataContext.GetType() == dataContextType)
                {
                    currentLevel++;
                    if (currentLevel <= maxMatchLevels)
                        return obj;
                    else return null;
                }
                obj = VisualTreeHelper.GetParent(obj);
            };
            return null;
        }

        public static ContextType FindAncestorByDataContext<ContextType>(DependencyObject obj, int maxMatchLevels, List<Type> noGoTypes)
        {
            int currentLevel = 0;
            while (obj != null)
            {
                var currentType = obj.GetType();
                if (noGoTypes != null && noGoTypes.Contains(currentType))
                    return default(ContextType);
                var dataContext = ((FrameworkElement)obj).DataContext;
                if (dataContext is ContextType)
                {
                    currentLevel++;
                    if (currentLevel <= maxMatchLevels)
                        return (ContextType)dataContext;
                    else return default(ContextType);
                }
                obj = VisualTreeHelper.GetParent(obj);
            };
            return default(ContextType);
        }

        public static bool DetectAncestorOfBadTypes(DependencyObject obj, List<Type> noGoTypes)
        {
            while (obj != null && obj is Visual)
            {
                var currentType = obj.GetType();
                if (noGoTypes.Contains(currentType))
                    return true;
                else obj = VisualTreeHelper.GetParent(obj);
            };
            return false;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static void MouseCursorWait()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        }

        public static void MouseCursorNormal()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }

        public static void DeleteUpdateLeftOvers()
        {
            var bakFile = Constants.AppName + ".exe.bak";
            var updateFile = Constants.AppName + ".exe.update";
            if (System.IO.File.Exists(bakFile))
                System.IO.File.Delete(bakFile);
            if (System.IO.File.Exists(updateFile))
                System.IO.File.Delete(updateFile);
        }

        public static decimal StringToDecimal(string input)
        {
            decimal localValue = 0;
            decimal.TryParse(input, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out localValue);
            return localValue;
        }

        public static string ApplicationPath()
        {
            return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }

        public static string ApplicationVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static bool RestartComputer(int delaySeconds)
        {
            var result = Process.Start("shutdown.exe", $"-r -t {delaySeconds}");
            return result != null ? true : false;
        }

        public static async void RestartApplication(int asyncDelayMilliseconds, int CmdDelaySeconds, bool shutdownApp)
        {
            if (asyncDelayMilliseconds != 0)
                await Task.Delay(asyncDelayMilliseconds);
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = $"/C TIMEOUT {CmdDelaySeconds} & START \"\" \"" + ApplicationPath() + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            if (shutdownApp)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }

        public static bool ListContainsThisPC(IList<string> list)
        {
            var first = list.FirstOrDefault(x => string.Equals(x, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase));
            return first != null ? true : false;
        }
    }
}

