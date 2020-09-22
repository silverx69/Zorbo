using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zorbo.Ares.Server;
using Zorbo.Ares.Server.Users;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginWin.xaml
    /// </summary>
    public partial class LoginWin : Window
    {
        CollectionView view;

        public LoginWin(AresUserHistory history) {

            DataContext = history;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            view = (CollectionView)CollectionViewSource.GetDefaultView(listBox1.ItemsSource);
            view.Filter = (obj) =>
                ((Record)obj).Name.Contains(txtSearch.Text) ||
                ((Record)obj).ClientId.ExternalIp.ToString().Contains(txtSearch.Text);
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
    }
}
