using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Zorbo.Ares.Server;
using Zorbo.Core;
using Zorbo.Core.Models;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        bool forced;

        readonly AresServer server;
        System.Windows.Forms.NotifyIcon icon;

        public Main() {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this)) {

                var config = Persistence.LoadModel<Configuration>(Path.Combine(Directories.AppData, "config.json"));

                server = new AresServer(config);
                server.PropertyChanged += Server_PropertyChanged;

                DataContext = server;
            }
        }

        private void ForceShow() {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ForceClose() {
            forced = true;
            this.Close();
        }

        private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Running") {

                if (server.Running)
                    btnStart.Content = "Stop";

                else {
                    btnStart.Content = "Start";
                    ((Configuration)server.Config).SaveModel(Path.Combine(Directories.AppData, "config.json"));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            icon = new System.Windows.Forms.NotifyIcon();

            System.Windows.Forms.ContextMenuStrip cm = new System.Windows.Forms.ContextMenuStrip();

            cm.Items.Add("Close", null, (s, a) => { ForceClose(); });

            icon.ContextMenuStrip = cm;
            icon.MouseClick += (s, args) => {
                if (args.Button == System.Windows.Forms.MouseButtons.Left) {
                    if (Visibility == Visibility.Hidden)
                        this.ForceShow();

                    else this.Hide();
                }
            };
            
            var stream = Application.GetResourceStream(
                new Uri("/Zorbo.Server.WPF;component/Zorbo.ico", UriKind.Relative)).Stream;

            if (stream != null)
                icon.Icon = new System.Drawing.Icon(stream);

            icon.Visible = true;

            server.PluginHost.LoadPlugin("Autoload");

            if (server.Config.AutoStartServer)
                server.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {

            if (!forced) {
                e.Cancel = true;
                this.Hide();
            }
            else {
                if (server.Running)
                    server.Stop();

                server.Config.SaveModel(Path.Combine(Directories.AppData, "config.json"));

                icon.Visible = false;
                icon.Dispose();
            }
        }


        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if (btnStart.Content.ToString() == "Start") {
                if (!server.Running)
                    server.Start();
            }
            else {
                if (server.Running)
                    server.Stop();
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e) {
            var settings = new Settings(server);

            settings.Closed += Settings_Closed;
            settings.Owner = this;
            settings.ShowDialog();
        }

        private void Settings_Closed(object sender, EventArgs e) {
            var settings = (Settings)sender;
            settings.Closed -= Settings_Closed;

            server.Config.SaveModel(Path.Combine(Directories.AppData, "config.json"));
        }
    }
}
