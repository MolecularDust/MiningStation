using System;
using System.ComponentModel;
using System.Windows;

namespace Mining_Station
{
    // Weak Event manager for PropertyChanging event.
    class PropertyChangingEventManager : WeakEventManager
    {
        private PropertyChangingEventManager()
        {
        }

        public static void AddHandler(INotifyPropertyChanging source,
                                      EventHandler<PropertyChangingEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        public static void RemoveHandler(INotifyPropertyChanging source,
                                         EventHandler<PropertyChangingEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        private static PropertyChangingEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(PropertyChangingEventManager);
                PropertyChangingEventManager manager =
                    (PropertyChangingEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new PropertyChangingEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected override ListenerList NewListenerList()
        {
            return new ListenerList<PropertyChangingEventArgs>();
        }

        protected override void StartListening(object source)
        {
            INotifyPropertyChanging typedSource = (INotifyPropertyChanging)source;
            typedSource.PropertyChanging += new PropertyChangingEventHandler(OnPropertyChanging);
        }

        protected override void StopListening(object source)
        {
            INotifyPropertyChanging typedSource = (INotifyPropertyChanging)source;
            typedSource.PropertyChanging -= new PropertyChangingEventHandler(OnPropertyChanging);
        }

        void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            DeliverEvent(sender, e);
        }
    }
}
