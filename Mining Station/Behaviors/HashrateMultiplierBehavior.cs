using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mining_Station
{
    class HashrateMultiplierBehavior : Behavior<ContextMenu>
    {
        MenuItem HashrateMultipier;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(HashrateMultiplierBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.AssociatedObject.Items.Contains(HashrateMultipier))
                return;
            HashrateMultipier = new MenuItem { Header = "Hashrate Multipier" };
            HashrateMultipier.Click += HashrateMultipier_Click;
            this.AssociatedObject.Items.Add(HashrateMultipier);

        }

        private void HashrateMultipier_Click(object sender, RoutedEventArgs e)
        {
            var dg = AssociatedObject.PlacementTarget as DataGrid;
            if (dg == null || dg.SelectedItem == null)
                return;
            Command.Execute(dg.SelectedItem);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
