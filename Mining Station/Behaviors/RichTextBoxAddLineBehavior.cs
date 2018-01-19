using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mining_Station
{
    public class RichTextBoxAddLineBehavior : Behavior<RichTextBox>
    {
        public string AddLine
        {
            get { return (string)GetValue(AddLineProperty); }
            set { SetValue(AddLineProperty, value); }
        }

        public static readonly DependencyProperty AddLineProperty =
            DependencyProperty.Register("AddLine", typeof(string), typeof(RichTextBoxAddLineBehavior), new PropertyMetadata(string.Empty, OnAddLineChanged));

        private static void OnAddLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as RichTextBoxAddLineBehavior;
            var rb = behavior.AssociatedObject as RichTextBox;
            rb.AppendText((string)e.NewValue);
        }

        public Paragraph AddParagraph
        {
            get { return (Paragraph)GetValue(AddParagraphProperty); }
            set { SetValue(AddParagraphProperty, value); }
        }

        public static readonly DependencyProperty AddParagraphProperty =
            DependencyProperty.Register("AddParagraph", typeof(Paragraph), typeof(RichTextBoxAddLineBehavior), new PropertyMetadata(null, OnAddParagraphChanged));

        private static void OnAddParagraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var behavior = d as RichTextBoxAddLineBehavior;
            var rb = behavior.AssociatedObject as RichTextBox;
            rb.Document.Blocks.Add((Paragraph)e.NewValue);
        }

        public Run AddRun
        {
            get { return (Run)GetValue(AddRunProperty); }
            set { SetValue(AddRunProperty, value); }
        }

        public static readonly DependencyProperty AddRunProperty =
            DependencyProperty.Register("AddRun", typeof(Run), typeof(RichTextBoxAddLineBehavior), new PropertyMetadata(null, OnAddRunChanged));

        private static void OnAddRunChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var behavior = d as RichTextBoxAddLineBehavior;
            var rb = behavior.AssociatedObject as RichTextBox;
            var last = rb.Document.Blocks.Last() as Paragraph;
            last.Inlines.Add((Run)e.NewValue);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Document.Blocks.Clear();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
