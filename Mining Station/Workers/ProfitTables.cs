using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ProfitTables : NotifyObject
    {
        public ObservableCollection<ProfitTable> Tables { get; set; }

        public RelayCommandLight ExpandAll { get; private set; }
        public RelayCommandLight CollapseAll { get; private set; }
        public RelayCommandLight ExpandAll_AllWorkers { get; private set; }
        public RelayCommandLight CollapseAll_AllWorkers { get; private set; }

        public RelayCommandLight SwitchAll { get; private set; }
        public RelayCommandLight SwitchNone { get; private set; }
        public RelayCommandLight SwitchAll_AllWorkers { get; private set; }
        public RelayCommandLight SwitchNone_AllWorkers { get; private set; }

        public RelayCommandLight RestartAll { get; private set; }
        public RelayCommandLight RestartNone { get; private set; }
        public RelayCommandLight RestartAll_AllWorkers { get; private set; }
        public RelayCommandLight RestartNone_AllWorkers { get; private set; }

        public ProfitTables()
        {
            ExpandAll = new RelayCommandLight(ExpandAllCommand);
            CollapseAll = new RelayCommandLight(CollapseAllCommand);
            ExpandAll_AllWorkers = new RelayCommandLight(ExpandAll_AllWorkersCommand);
            CollapseAll_AllWorkers = new RelayCommandLight(CollapseAll_AllWorkersCommand);

            SwitchAll = new RelayCommandLight(SwitchAllCommand);
            SwitchNone = new RelayCommandLight(SwitchNoneCommand);
            SwitchAll_AllWorkers = new RelayCommandLight(SwitchAll_AllWorkersCommand);
            SwitchNone_AllWorkers = new RelayCommandLight(SwitchNone_AllWorkersCommand);

            RestartAll = new RelayCommandLight(RestartAllCommand);
            RestartNone = new RelayCommandLight(RestartNoneCommand);
            RestartAll_AllWorkers = new RelayCommandLight(RestartAll_AllWorkersCommand);
            RestartNone_AllWorkers = new RelayCommandLight(RestartNone_AllWorkersCommand);

        }

        private void Restart(bool flag, object source)
        {
            var profitTable = source as ProfitTable;
            if (profitTable == null) return;
            foreach (var pc in profitTable.Computers)
                pc.Restart = flag;
        }

        private void RestartAllWorkers(bool flag)
        {
            foreach (var table in this.Tables)
                Restart(flag, table);
        }

        private void RestartAllCommand(object obj)
        {
            Restart(true, obj);
        }

        private void RestartNoneCommand(object obj)
        {
            Restart(false, obj);
        }

        private void RestartAll_AllWorkersCommand(object obj)
        {
            RestartAllWorkers(true);
        }

        private void RestartNone_AllWorkersCommand(object obj)
        {
            RestartAllWorkers(false);
        }

        private void Switch(bool flag, object source)
        {
            var profitTable = source as ProfitTable;
            if (profitTable == null) return;
            foreach (var pc in profitTable.Computers)
                pc.Switch = flag;
        }

        private void SwitchAllWorkers(bool flag)
        {
            foreach (var table in this.Tables)
                Switch(flag, table);
        }

        private void SwitchAllCommand(object obj)
        {
            Switch(true, obj);
        }

        private void SwitchNoneCommand(object obj)
        {
            Switch(false, obj);
        }

        private void SwitchAll_AllWorkersCommand(object obj)
        {
            SwitchAllWorkers(true);
        }

        private void SwitchNone_AllWorkersCommand(object obj)
        {
            SwitchAllWorkers(false);
        }

        private void Expand(bool flag, object source)
        {
            var profitTable = source as ProfitTable;
            if (profitTable == null) return;
            foreach (var pc in profitTable.Computers)
                pc.IsExpanded = flag;
        }

        private void ExpandAllWorkers(bool flag)
        {
            foreach (var table in this.Tables)
                Expand(flag, table);
        }

        private void ExpandAllCommand(object obj)
        {
            Expand(true, obj);
        }

        private void CollapseAllCommand(object obj)
        {
            Expand(false, obj);
        }

        private void ExpandAll_AllWorkersCommand(object obj)
        {
            ExpandAllWorkers(true);
        }

        private void CollapseAll_AllWorkersCommand(object obj)
        {
            ExpandAllWorkers(false);
        }
    }
}
