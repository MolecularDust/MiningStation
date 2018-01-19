using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {

        public void SaveWtmSettings()
        {
            string json = JsonConverter.ConvertToJson(WtmSettings);
            string jsonFormatted = JsonConverter.FormatJson(json);
            Helpers.WriteToTxtFile(Constants.WtmSettingsFile, jsonFormatted);
        }

        private void SaveCommand(object parameter)
        {
            Debug.WriteLine("Save button command.");

            Workers.SaveWorkers(Workers);

            ApplyWtmSettingsAndSave();
        }
    }
}
