using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace Mining_Station
{
    public class TextBlockBindableInlinesBehavior : Behavior<TextBlock>
    {
        // This accumulator is needed when binding occures before the behavior is attached to control
        private IList<Inline> Cache;

        public IList<Inline> BindableInlines
        {
            get { return (IList<Inline>)GetValue(BindableInlinesProperty); }
            set { SetValue(BindableInlinesProperty, value); }
        }

        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.Register("BindableInlines", typeof(IList<Inline>), typeof(TextBlockBindableInlinesBehavior), new PropertyMetadata(new List<Inline>(), OnBindableInlinesChanged));

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;

            var control = d as TextBlockBindableInlinesBehavior;
            if (control.AssociatedObject == null)
            {
                control.Cache = new List<Inline>();
                foreach (var inline in (IList<Inline>) e.NewValue)
                    control.Cache.Add(inline);
                return;
            };

            control.AssociatedObject.Inlines.Clear();
            foreach (var inline in (IList<Inline>)e.NewValue)
                control.AssociatedObject.Inlines.Add(inline);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (Cache != null)
            {
                SetInlines(this, Cache);
            }
        }

        private void SetInlines(TextBlockBindableInlinesBehavior control, IList<Inline> list)
        {
            control.AssociatedObject.Inlines.Clear();
            foreach (var inline in list)
                control.AssociatedObject.Inlines.Add(inline);
        }
    }
}
