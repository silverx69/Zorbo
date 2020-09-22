using System.Windows;
using System.Windows.Controls;
using Zorbo.Ares.Server;
using Zorbo.Ares.Server.Users;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for Logins.xaml
    /// </summary>
    public partial class Logins : UserControl
    {
        public Logins() {
            InitializeComponent();
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e) {
            LoginWin win = new LoginWin((AresUserHistory)DataContext) {
                Owner = this.FindVisualAnscestor<Window>()
            };
            win.ShowDialog();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e) {

            if (lstCurrentAdmin.SelectedIndex > -1)
                ((AresUserHistory)DataContext).Admin.Passwords.RemoveAt(lstCurrentAdmin.SelectedIndex);
        }
    }
}
