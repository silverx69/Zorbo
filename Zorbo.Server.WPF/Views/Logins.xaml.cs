using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zorbo.Ares.Server;
using Zorbo.Ares.Server.Users;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for Logins.xaml
    /// </summary>
    public partial class Logins : UserControl
    {
        public AresUserHistory History {
            get { return (AresUserHistory)DataContext; }
        }

        public CollectionViewSource Passwords {
            get => (CollectionViewSource)base.GetValue(PasswordsProperty);
            set => base.SetValue(PasswordsProperty, value);
        }

        public Logins() {
            InitializeComponent();
            Passwords = new CollectionViewSource();
            Loaded += Logins_Loaded;
        }

        private void Logins_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                History.Admin.Passwords.CollectionChanged += Passwords_CollectionChanged;
                Passwords.Source = History.Admin.Passwords.ToArray();
            }
        }

        private void Passwords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() => { Passwords.Source = History.Admin.Passwords.ToArray(); });
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

        public static DependencyProperty PasswordsProperty =
            DependencyProperty.Register(
                "Passwords",
                typeof(CollectionViewSource),
                typeof(Logins),
                new FrameworkPropertyMetadata());
    }
}
