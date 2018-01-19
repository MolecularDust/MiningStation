using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mining_Station.History;

namespace Mining_Station
{
    // Redo / Undo stuff
    public partial class ViewModel : NotifyObject
    {
        private UndoObject SaveState(string propertyName)
        {
            UndoObject undoObject = null;
            var index = Workers.WorkerIndex;
            switch (propertyName)
            {
                case "WorkersPowerCost":
                    undoObject = new UndoObject(UndoOperationType.WorkersPowerCost, -1, Workers.PowerCost);
                    break;
                case "WorkersCoinType":
                    undoObject = new UndoObject(UndoOperationType.WorkersCoinType, -1, Workers.CoinType);
                    break;
                case "WorkersDisplayCoinAs":
                    undoObject = new UndoObject(UndoOperationType.WorkersDisplayCoinAs, -1, Workers.DisplayCoinAs);
                    break;
                case "WorkerList":
                    undoObject = new UndoObject(UndoOperationType.WorkerEdit, index, Workers.WorkerList[index].CloneNoEvents());
                    break;
                case "WorkerAdd":
                    undoObject = new UndoObject(UndoOperationType.WorkerAdd, -1, null);
                    break;
                case "WorkerInsert":
                    undoObject = new UndoObject(UndoOperationType.WorkerInsert, index, Workers.WorkerList[index].CloneNoEvents());
                    break;
                case "WorkerRemove":
                    undoObject = new UndoObject(UndoOperationType.WorkerRemove, index, Workers.WorkerList[index].CloneNoEvents());
                    break;
                case "WorkerMove":
                    undoObject = new UndoObject(UndoOperationType.WorkerMove, index, Workers.WorkerNewIndex);
                    break;
                case "WtmSettings":
                    undoObject = new UndoObject(UndoOperationType.SettingsEdit, -1, WtmSettings.Clone());
                    break;
            }
            return undoObject;
        }

        private void RestoreState(UndoObject undoObject)
        {
            switch (undoObject.OperationType)
            {
                case UndoOperationType.WorkerEdit:
                    Workers.WorkerListReplaceItem(undoObject.Index, ((Worker)undoObject.Data).Clone());
                    break;
                case UndoOperationType.WorkersPowerCost:
                    Workers.PowerCost = (double)undoObject.Data;
                    break;
                case UndoOperationType.WorkersCoinType:
                    Workers.CoinType = (string)undoObject.Data;
                    break;
                case UndoOperationType.WorkersDisplayCoinAs:
                    Workers.DisplayCoinAs = (string)undoObject.Data;
                    break;
                case UndoOperationType.WorkerAdd:
                    Workers.WorkerListRemoveAt(Workers.WorkerList.Count - 1);
                    break;
                case UndoOperationType.WorkerInsert:
                    Workers.WorkerListRemoveAt(undoObject.Index);
                    break;
                case UndoOperationType.WorkerRemove:
                    Workers.WorkerListInsert(undoObject.Index, ((Worker)undoObject.Data).Clone());
                    break;
                case UndoOperationType.WorkerMove:
                    Workers.WorkerListMove((int)undoObject.Data, undoObject.Index);
                    break;
                case UndoOperationType.SettingsEdit:
                    UnHookUpWmtSettingsPropertyChangeEvents();
                    SaveUndoRedo("WtmSettings");
                    var wtms = ((WtmSettingsObject)undoObject.Data).Clone();
                    bool switchTimeFromEqual = WtmSwitchTimeFromStored.Equals(wtms.SwitchTimeFrom);
                    bool switchTimeToEqual = WtmSwitchTimeToStored.Equals(wtms.SwitchTimeTo);
                    bool historicalAverageEqual = WtmHistoricalAveragePeriodStored == wtms.HistoricalAveragePeriod;
                    bool historyTimeFromEqual = WtmHistoryTimeFromStored.Equals(wtms.HistoryTimeFrom);
                    bool historyTimeToEqual = WtmHistoryTimeToStored.Equals(wtms.HistoryTimeTo);
                    WtmSettings = wtms;
                    HookUpWmtSettingsPropertyChangeEvents();
                    if (WtmSettings.AutoSwitch)
                        SwitchTimeIsUpdated = false;
                    if (WtmSettings.BackupHistoricalPrices)
                        HistoryTimeIsUpdated = false;
                    WtmSettings.ServerSettingsAreUpdated = true;
                    break;
            }
        }

        private void SaveUndoRedo(string name)
        {
            if (!SaveUndoIsEnabled)
                return;
            var undoObject = SaveState(name);
            if (History.UndoInProgress)
                History.SaveRedo(undoObject);
            else
            {
                History.SaveUndo(undoObject);
                if (!History.RedoInProgress)
                    History.RedoClear();
            }
        }

        private void UndoCommand(object parameter)
        {
            Debug.WriteLine("Undo in progress.");
            if (History.IsUndoable)
            {
                Debug.WriteLine("Undo: " + " Current property " + " Redo stack count ");
                History.UndoInProgress = true;
                RestoreState(History.Undo());
                History.UndoInProgress = false;
            }
        }

        private void RedoCommand(object parameter)
        {
            Debug.WriteLine("Redo in progress.");

            if (History.IsRedoable)
            {
                History.RedoInProgress = true;
                RestoreState(History.Redo());
                History.RedoInProgress = false;
            }
        }

    }
}
