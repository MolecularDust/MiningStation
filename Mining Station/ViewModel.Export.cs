using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private bool Export_CanExecute(object obj)
        {
            if (ProfitTables.Tables.Count > 0)
                return true;
            else return false;
        }

        private void ExportCommand(object parameter)
        {
            bool emptyLineRequired = (ProfitTables.Tables.Count > 1);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file|*.csv|JSON file|*.json|XML File|*.xml";
            saveFileDialog.OverwritePrompt = true;
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == false)
                return;
            string path = saveFileDialog.FileName;
            string extension = Path.GetExtension(path).ToLower();
            StreamWriter writer = null;
            try { writer = new StreamWriter(path); }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (writer != null)
            {
                try
                {
                    var selectedWorkers = Workers.WorkerList.Where(x => x.Query).Select(x => x.Name);
                    var selectedTables = ProfitTables.Tables.Where(x => selectedWorkers.Contains(x.Name));

                    switch (extension)
                    {
                        case ".csv":
                            writer.WriteLine("Values in," + WtmSettings.DisplayCurrency);
                            writer.WriteLine();

                            foreach (var table in selectedTables)
                            {
                                writer.WriteLine("Worker," + "\"" + table.Name + "\"");
                                writer.WriteLine("Description," + "\"" + table.Description + "\"");
                                writer.WriteLine("Computers," + "\"" + string.Join(", ", table.ComputerNames) + "\"");
                                writer.WriteLine("Name," + "Symbol," + "Algorithm," + "Hashrate," + "Path," + "Arguments," + "Revenue," + "Profit Day," + "Profit Week," + "Profit Month," + "Profit Year," + "Notes,");
                                foreach (var list in table.ProfitList)
                                {
                                    if (WtmSettings.DisplayCurrency != "USD")
                                    {
                                        var rate = WtmSettings.DisplayCurrencyList[WtmSettings.DisplayCurrency];
                                        list.Revenue /= rate;
                                        list.ProfitDay /= rate;
                                        list.ProfitWeek /= rate;
                                        list.ProfitMonth /= rate;
                                        list.ProfitYear /= rate;
                                    }
                                    var line = $"\"{list.Name}\",\"{list.Symbol}\",\"{list.Algorithm}\",\"{list.Hashrate}\",\"{list.Path}\",\"{list.Arguments}\",\"{list.Revenue}\",\"{list.ProfitDay}\",\"{list.ProfitWeek}\",\"{list.ProfitMonth}\",\"{list.ProfitYear}\",\"{list.Notes}\"";
                                    writer.WriteLine(line);
                                }
                                if (emptyLineRequired) writer.WriteLine("");
                                writer.Flush();
                            }
                            break;
                        case ".json":
                            var json = JsonConverter.ConvertToJson(selectedTables);
                            json = JsonConverter.FormatJson(json);
                            writer.Write(json);
                            break;
                        case ".xml":
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ProfitTable>));
                            xmlSerializer.Serialize(writer, selectedTables.ToList());
                            break;
                    }
                }
                catch (Exception e) { MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { writer.Dispose(); }
            }
        }
    }
}
