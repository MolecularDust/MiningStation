using System.Collections.Generic;

namespace Mining_Station
{
    // Undo-redo history stacks and methods

    public class History : NotifyObject
    {
        public enum UndoOperationType
        {
            WorkerEdit,
            WorkerAdd,
            WorkerAddRange,
            WorkerInsert,
            WorkerRemove,
            WorkerRemoveRange,
            WorkerMove,
            WorkersPowerCost,
            WorkersCoinType,
            WorkersDisplayCoinAs,
            SettingsEdit,
        }

        public class UndoObject
        {
            public UndoOperationType OperationType { get; set; }
            public int Index { get; set; }
            public int Shift { get; set; }
            public object Data { get; set; }


            public UndoObject(UndoOperationType operationType, int index, object data)
            {
                this.OperationType = operationType;
                this.Index = index;
                this.Data = data;
            }

            public UndoObject(UndoObject undoObject)
            {
                this.OperationType = undoObject.OperationType;
                this.Index = undoObject.Index;
                this.Shift = undoObject.Shift;
                this.Data = undoObject.Data;

            }
        }

        public bool UndoInProgress { get; set; }
        public bool RedoInProgress { get; set; }
        public Stack<UndoObject> UndoStack;
        public Stack<UndoObject> RedoStack;

        private bool _isUndoable;
        public bool IsUndoable
        {
            get { return _isUndoable; }
            set { _isUndoable = value; OnPropertyChanged("IsUndoable"); }
        }
        private bool _isRedoable;
        public bool IsRedoable
        {
            get { return _isRedoable; }
            set { _isRedoable = value; OnPropertyChanged("IsRedoable"); }
        }

        public History()
        {
            this.UndoStack = new Stack<UndoObject>();
            this.RedoStack = new Stack<UndoObject>();
            this.IsRedoable = false;
            this.IsUndoable = false;
        }

        public void SaveUndo(UndoObject undoObject)
        {
            this.UndoStack.Push(undoObject);
            this.IsUndoable = true;
        }

        public void SaveRedo(UndoObject redoObject)
        {
            this.RedoStack.Push(redoObject);
            this.IsRedoable = true;
        }

        public UndoObject Undo()
        {
            var undoObject = UndoStack.Pop();
            this.IsRedoable = true;
            if (UndoStack.Count == 0)
                this.IsUndoable = false;
            return undoObject;
        }

        public UndoObject Redo()
        {
            var undoObject = RedoStack.Pop();
            this.IsUndoable = true;
            if (RedoStack.Count == 0)
                this.IsRedoable = false;
            return undoObject;
        }

        public void RedoClear()
        {
            this.RedoStack.Clear();
            this.IsRedoable = false;
        }
    }
}
