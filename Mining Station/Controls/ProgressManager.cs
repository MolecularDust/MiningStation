using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mining_Station
{
    public class ProgressManager
    {
        private ProgressWindow window;
        private DispatcherTimer timer;
        public bool IsAlive;
        CancellationTokenSource cancelSource;

        public ProgressManager() { }

        public ProgressManager(string message)
        {
            window = new ProgressWindow();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Status.Text = message;
            window.Closed += new EventHandler(Window_Closed);
            IsAlive = true;
            window.Show();
        }

        public ProgressManager(string message, CancellationTokenSource tokenSource, int delay = 0, bool indeterminate = false)
        {
            cancelSource = tokenSource;
            window = new ProgressWindow();
            IsAlive = true;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(delay);
            timer.Tick += ((s, e) => {
                timer.Stop();
                window.Closed += new EventHandler(Window_Closed);
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Status.Text = message;
                window.Progress.IsIndeterminate = indeterminate;
                if (this.window != null && IsAlive)
                {
                    this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => this.window.Show() ));
                }
            });
            timer.Start();
        }

        public void Close()
        {
            IsAlive = false;
            bool isLoaded = false;
            window.Dispatcher.Invoke((Action)(() => isLoaded = window.IsLoaded ));
            if (window != null && isLoaded)
            {
                window.Dispatcher.Invoke((Action)(() => window.Close() ));
            }
        }

        public void SetText(string text)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke((Action)(() => window.Status.Text = text ));
            }
        }

        public void SetProgressValue(double value)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke((Action)(() => window.Progress.Value = value ));
            }
        }

        public void SetProgressMaxValue(double maxValue)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke((Action)(() =>
                {
                    window.Progress.Minimum = 0;
                    window.Progress.Maximum = maxValue;
                }));
            }
        }

        public void SetIndeterminate(bool b)
        {
            if (window != null)
            {
                window.Dispatcher.Invoke((Action)(() => window.Progress.IsIndeterminate = b ));
            }
        }

        void Window_Closed(object sender, EventArgs e)
        {
            this.IsAlive = false;
            if (cancelSource != null)
                cancelSource.Cancel();
        }

    }

    //public class ProgressManager
    //{
    //    private Thread thread;
    //    private volatile bool canAbortThread = false;
    //    private ProgressWindow window;
    //    private string DefaultMessage;
    //    public bool IsAlive { get; set; }
    //    private CancellationTokenSource cancelTokenSource;
    //    private int startDelay;
    //    private bool isIndeterminate;


    //    public ProgressManager() { }
    //    public ProgressManager(string initialMessage)
    //    {
    //        DefaultMessage = initialMessage;
    //        IsAlive = true;
    //    }

    //    public ProgressManager(string initialMessage, CancellationTokenSource tokenSource, int delay = 0, bool indeterminate = false )
    //    {
    //        DefaultMessage = initialMessage;
    //        IsAlive = true;
    //        cancelTokenSource = tokenSource;
    //        startDelay = delay;
    //        isIndeterminate = indeterminate;
    //    }

    //    public void BeginWaiting()
    //    {
    //        this.thread = new Thread(this.RunThread);
    //        this.thread.IsBackground = true;
    //        this.thread.SetApartmentState(ApartmentState.STA);
    //        this.thread.Start();
    //    }
    //    public void EndWaiting()
    //    {
    //        if (this.window != null)
    //        {
    //            this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    //            { this.window.Close(); }));
    //            while (!this.canAbortThread) { };
    //        }
    //        //this.thread.Interrupt();
    //    }

    //    public void RunThread()
    //    {
    //        if (startDelay != 0)
    //            Thread.Sleep(1000);
    //        if (!IsAlive)
    //            return;
    //        this.window = new ProgressWindow();
    //        this.window.Topmost = true;
    //        if (DefaultMessage != string.Empty)
    //            this.window.Status.Text = DefaultMessage;
    //        if (isIndeterminate)
    //            this.window.Progress.IsIndeterminate = true;
    //        this.window.Closed += new EventHandler(waitingWindow_Closed);
    //        this.window.ShowDialog();

    //    }
    //    public void ChangeStatus(string text)
    //    {
    //        if (this.window != null)
    //        {
    //            this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    //            { this.window.Status.Text = text; }));
    //        }
    //    }
    //    public void ChangeProgress(double Value)
    //    {
    //        if (this.window != null)
    //        {
    //            this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    //            { this.window.Progress.Value = Value; }));
    //        }
    //    }
    //    public void SetProgressMaxValue(double MaxValue)
    //    {
    //        Thread.Sleep(100);
    //        if (this.window != null)
    //        {
    //            this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    //            {
    //                this.window.Progress.Minimum = 0;
    //                this.window.Progress.Maximum = MaxValue;
    //            }));
    //        }
    //    }
    //    public void SetIndeterminate(bool b)
    //    {
    //        Thread.Sleep(100);
    //        if (this.window != null)
    //        {
    //            this.window.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    //            { this.window.Progress.IsIndeterminate = b; }));
    //        }
    //    }
    //    void waitingWindow_Closed(object sender, EventArgs e)
    //    {
    //        this.IsAlive = false;
    //        this.canAbortThread = true;
    //        if (cancelTokenSource != null)
    //            cancelTokenSource.Cancel();
    //        Dispatcher.CurrentDispatcher.InvokeShutdown();
    //    }
    //}
}
