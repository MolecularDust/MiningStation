using FluentScheduler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Mining_Station
{
    public partial class App : Application
    {
        public enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        [DllImportAttribute("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowToFront(string windowName)
        {
            IntPtr firstInstance = FindWindow(null, windowName);
            ShowWindow(firstInstance, ShowWindowEnum.ShowNormal);
            SetForegroundWindow(firstInstance);
        }

        private static Mutex _mutex = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool createdNew;

            _mutex = new Mutex(true, Constants.AppName, out createdNew);

            if (!createdNew)
            {
                ShowToFront(Constants.AppName);
                Application.Current.Shutdown();
                return;
            }

            // Set local culture for the entire app
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Set app's current directory
            string path = Helpers.ApplicationPath();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));

            // Check if Mining Station registry keys exist and create such if they don't. 
            using (RegistryKey rk = Microsoft.Win32.Registry.CurrentUser)
            {
                try
                {
                    string baseKey = "SOFTWARE\\Mining Station";
                    if (rk.OpenSubKey(baseKey, true) == null)
                        rk.CreateSubKey(baseKey);

                    List<string> subKeys = new List<string> { "\\Switch", "\\UpdatePriceHistory" };
                    foreach (var subKey in subKeys)
                    {
                        string currentSubKey = baseKey + subKey;
                        if (rk.OpenSubKey(currentSubKey, true) == null)
                        {
                            rk.CreateSubKey(currentSubKey);
                            var sk = rk.OpenSubKey(currentSubKey, true);
                            sk.SetValue("IsInProgress", false);
                            sk.SetValue("Round", 0);
                            sk.SetValue("Schedule", string.Empty);
                            sk.SetValue("LastUpdate", string.Empty);
                            sk.SetValue("LastSuccess", string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while accessing registry.\n" + ex.Message);
                }
            }
        }

        //Dispose mutex on exit
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex.Dispose();
            base.OnExit(e);
        }

        // Generic unhandled error handler
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var dialogResult = MessageBox.Show($"{Constants.AppName} is now in limbo, click 'OK' to euthanize it or 'Cancel' to hope for a miracle.\n\n" + e.Exception.Message, "Unknown error occurred", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            if (dialogResult == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
