using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for NetworkTLS.xaml
    /// </summary>
    public partial class NetworkTLS : UserControl
    {
        private IServerConfig Config {
            get { return DataContext as IServerConfig; }
        }

        public NetworkTLS()
        {
            InitializeComponent();
            Loaded += Network_Loaded;
        }

        private void Network_Loaded(object sender, RoutedEventArgs e)
        {
            Config.PropertyChanged += Config_PropertyChanged;
            passBox.Password = Config.CertificatePassword.ToNativeString();
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Help help = this.FindVisualAnscestor<SettingsWin>().Help;

            help.Control = sender as UIElement;
            help.Text = Help.GetHelpText(help.Control);

            help.IsOpen = true;
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (e.PropertyName == nameof(Config.CertificatePassword))
                    passBox.Password = Config.CertificatePassword.ToNativeString();
            });
        }

        private void PassBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Config.CertificatePassword = passBox.SecurePassword;
        }
    }


}
