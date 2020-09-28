using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for Website.xaml
    /// </summary>
    public partial class Website : UserControl
    {
        public Website()
        {
            InitializeComponent();
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Help help = this.FindVisualAnscestor<Settings>().Help;

            help.Control = sender as UIElement;
            help.Text = Help.GetHelpText(help.Control);

            help.IsOpen = true;
        }
    }
}
