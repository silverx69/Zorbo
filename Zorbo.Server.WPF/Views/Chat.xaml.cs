using Microsoft.Win32;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zorbo.Ares;
using Zorbo.Ares.Server;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        public Chat() {
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e) {

            StringBuilder filter = new StringBuilder();

            filter.Append("Image Files (*.bmp, *.jpg, *.jpeg, *.gif, *.tif)|*.bmp;*.jpg;*.jpeg;*.gif;*.tif|");
            filter.Append("Bitmap Image (*.bmp)|*.bmp|");
            filter.Append("JPEG Image (*.jpg, *.jpeg)|*.jpg;*.jpeg|");
            filter.Append("GIF Image (*.gif)|*.gif|");
            filter.Append("TIFF Image (*.tif)|*.tif|");
            filter.Append("All Files (*.*)|*.*");

            OpenFileDialog ofd = new OpenFileDialog {
                Multiselect = false,
                CheckFileExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = filter.ToString()
            };

            if ((bool)ofd.ShowDialog()) {
                string file = ofd.FileName;

                AresAvatar avatar = AresAvatar.Load(file);
                ((AresServerConfig)DataContext).Avatar = avatar;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) {
            ((AresServerConfig)DataContext).Avatar = null;
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e) {

            Help help = this.FindVisualAnscestor<Settings>().Help;

            help.Control = sender as UIElement;
            help.Text = Help.GetHelpText(help.Control);

            help.IsOpen = true;
        }
    }
}
