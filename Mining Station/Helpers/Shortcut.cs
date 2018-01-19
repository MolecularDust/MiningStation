using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class Shortcut
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string StartIn { get; set; }
        public string Comments { get; set; }

        public Shortcut() {}

        public Shortcut(string path, string arguments, string startIn, string comments)
        {
            this.Path = path ?? string.Empty;
            this.Arguments = arguments ?? string.Empty;
            this.StartIn = startIn ?? string.Empty;
            this.Comments = comments ?? string.Empty;
        }

        //Interpret "AbcCoin, ABC, AbcAlgo"
        public static string[] SplitCoinInfo(string text)
        {
            var split = text.Split(',');
            for (int i = 0; i < split.Length; i++)
                split[i] = split[i].Trim();
            return split;
        }

        public string GetName()
        {
            if (this.Comments == null || this.Comments == string.Empty)
                return string.Empty;
            var split = SplitCoinInfo(this.Comments);
            return split.Length >= 1 ? split[0] : string.Empty;
        }

        public string GetSymbol()
        {
            if (this.Comments == null || this.Comments == string.Empty)
                return string.Empty;
            var split = SplitCoinInfo(this.Comments);
            return split.Length >= 2 ? split[1] : string.Empty;
        }

        public string GetAlgorithm()
        {
            if (this.Comments == null || this.Comments == string.Empty)
                return string.Empty;
            var split = SplitCoinInfo(this.Comments);
            return split.Length >= 3 ? split[2] : string.Empty;
        }

        public string GetNameAndSymbol()
        {
            if (this.Comments == null || this.Comments == string.Empty)
                return string.Empty;
            var split = SplitCoinInfo(this.Comments);
            return split.Length >= 2 ? $"{split[0]} ({split[1]})" : string.Empty;
        }

        public string GetCoinInfo(string displayCoinAs)
        {
            if (this.Comments == null || this.Comments == string.Empty)
                return string.Empty;
            var split = SplitCoinInfo(this.Comments);
            switch (displayCoinAs)
            {
                case "Name":
                    return split.Length >= 1 ? split[0] : string.Empty;
                case "SYMBOL":
                    return split.Length >= 2 ? split[1] : string.Empty;
                case "Name (SYMBOL)":
                    return split.Length >= 2 ? $"{split[0]} ({split[1]})" : string.Empty;
                default: return string.Empty;
            }
        }

        // Read current coin from shortcut in user's startup folder
        public static Shortcut GetCurrentCoin()
        {
            WshShell shell = new WshShell();
            string[] files = null;
            string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            files = Directory.GetFiles(startup, "start-miner-*.lnk");
            if (files.Length > 0)
            {
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(files[0]);
                return new Shortcut(shortcut.TargetPath, shortcut.Arguments, shortcut.WorkingDirectory, shortcut.Description);
            }
            else return null;
        }

        // Create a shortcut in user's startup folder
        public static void CreateCoinShortcut(string coinName, string coinSymbol, string coinAlgorithm, string path, string arguments)
        {
            WshShell shell = new WshShell();
            string[] files = null;
            string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            files = Directory.GetFiles(startup, "start-miner-*.lnk");
            if (files.Length > 0)
            {
                foreach (var file in files)
                    System.IO.File.Delete(file);
            }
            string link = startup + "\\" + "start-miner-" + coinSymbol + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(link);
            shortcut.TargetPath = path;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            shortcut.Description = $"{coinName}, {coinSymbol}, {coinAlgorithm}";
            shortcut.Save();
        }
    }
}
