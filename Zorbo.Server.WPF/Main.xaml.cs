using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
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

                var directories = new Directories();

                Logging.LogDirectory = directories.Logs;

                var config = Persistence.LoadModel<Configuration>(Path.Combine(directories.AppData, "config.json"));

                //supplying custom directories
                config.Directories = directories;
                config.PropertyChanged += Config_PropertyChanged;

                server = new AresServer(config);
                server.PropertyChanged += Server_PropertyChanged;

                DataContext = server;

                JumpList list = new JumpList {
                    ShowRecentCategory = false,
                    ShowFrequentCategory = false
                };

                list.JumpItems.Add(new JumpTask() {
                    Title = "Start server",
                    Arguments = "-start_server",
                    ApplicationPath = App.Filename,
                    WorkingDirectory = Directories.BaseDirectory,
                });

                list.JumpItems.Add(new JumpTask() {
                    Title = "Stop server",
                    Arguments = "-stop_server",
                    ApplicationPath = App.Filename,
                    WorkingDirectory = Directories.BaseDirectory,
                });

                list.JumpItems.Add(new JumpTask() {
                    Title = "Logs",
                    ApplicationPath = config.Directories.Logs,
                    CustomCategory = "Places"
                });

                list.JumpItems.Add(new JumpTask() {
                    Title = "Plugins",
                    ApplicationPath = config.Directories.Plugins,
                    CustomCategory = "Places"
                });

                JumpList.SetJumpList(Application.Current, list);
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

        private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            if (e.PropertyName == "Running") {

                if (server.Running)
                    btnStart.Content = "Stop";

                else {
                    btnStart.Content = "Start";
                    server.Config.SaveModel(Path.Combine(server.Config.Directories.AppData, "config.json"));
                }
            }
        }

        private bool is_resetting;
        private async void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoadWithWindows" && sender is Configuration config) {
                if (config.LoadWithWindows) {
                    if (!await LaunchAdminCommand("-add_start_win")) {
                        is_resetting = true;
                        config.LoadWithWindows = false;
                    }
                }
                else if (!is_resetting)
                    await LaunchAdminCommand("-rem_start_win");

                else is_resetting = false;
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

                server.Config.SaveModel(Path.Combine(server.Config.Directories.AppData, "config.json"));

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

            server.Config.SaveModel(Path.Combine(server.Config.Directories.AppData, "config.json"));
        }

        private static async Task<bool> LaunchAdminCommand(params string[] args)
        {
            try {
                await Task.Run(() => {
                    Process proc = new Process();
                    proc.StartInfo.FileName = App.Filename;
                    proc.StartInfo.Arguments = args.Join(" ");
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.Start();

                });
                return true;
            }
            catch (Exception ex) {
                MessageBox.Show(
                    string.Format("An error occured while running an elevated command.\r\nException: {0}", ex.Message),
                    "Elevation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}
