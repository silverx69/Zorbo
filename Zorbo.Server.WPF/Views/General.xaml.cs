using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zorbo.Core;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : UserControl
    {
        private Configuration Config {
            get { return (Configuration)DataContext; }
        }

        public General() {
            InitializeComponent();
            DataObject.AddCopyingHandler(txtTopic, Topic_OnCopy);
            DataObject.AddPastingHandler(txtTopic, Topic_OnPaste);
        }

        private void Topic_OnCopy(object sender, DataObjectCopyingEventArgs e) {
            e.Handled = true;
            e.CancelCommand();
            Clipboard.SetText(txtTopic.SelectedText.ToAresColor());
        }

        private void Topic_OnPaste(object sender, DataObjectPastingEventArgs e) {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
                return;

            e.Handled = true;
            e.CancelCommand();

            string text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            text = text.ToAresColor().ToReadableColor();

            txtTopic.SelectedText = text;
        }

        private void txtTopic_TextChanged(object sender, TextChangedEventArgs e)
        {
            //txtTopic.Text = txtTopic.Text.ToAresColor().ToReadableColor();
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e) {
            Help help = this.FindVisualAnscestor<SettingsWin>().Help;

            help.Control = sender as UIElement;
            help.Text = Help.GetHelpText(help.Control);

            help.IsOpen = true;
        }
    }
}
