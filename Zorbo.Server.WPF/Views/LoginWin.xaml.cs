using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zorbo.Ares.Server;
using Zorbo.Ares.Server.Users;
using Zorbo.Core;
using Zorbo.Core.Server;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginWin.xaml
    /// </summary>
    public partial class LoginWin : Window
    {
        ICollectionView view;

        public AresUserHistory History {
            get { return (AresUserHistory)DataContext; }
        }

        public CollectionViewSource Records {
            get => (CollectionViewSource)base.GetValue(RecordsProperty);
            set => base.SetValue(RecordsProperty, value);
        }

        public LoginWin(AresUserHistory history) {

            DataContext = history;
            InitializeComponent();
            Records = new CollectionViewSource();
            Loaded += OnLoaded;
        }

        private void InitSource() 
        {
            Records.Source = History.Records.ToArray();
            view = Records.View;
            view.Filter = (obj) =>
                ((Record)obj).Name.Contains(txtSearch.Text) ||
                ((Record)obj).ClientId.ExternalIp.ToString().Contains(txtSearch.Text);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                History.Records.CollectionChanged += Records_CollectionChanged;
                InitSource();
            }
        }

        private void Records_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(InitSource);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            view.Refresh();
        }

        private void Button1_Click(object sender, RoutedEventArgs e) {

            if (listBox1.SelectedIndex > -1) {
                var record = (Record)listBox1.SelectedItem;
                var history = (AresUserHistory)DataContext;

                history.Admin.Passwords.Add(new Password(record, (AdminLevel)(cbLevel.SelectedIndex + 1), passBox.SecurePassword));
            }

            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public static DependencyProperty RecordsProperty =
            DependencyProperty.Register(
                "Records",
                typeof(CollectionViewSource),
                typeof(LoginWin),
                new FrameworkPropertyMetadata());
    }
}
