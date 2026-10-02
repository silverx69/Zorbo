using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shell;
using Zorbo.Ares.Server;
using Zorbo.Core;
using Zorbo.Core.Models;
using Zorbo.Core.Plugins.Server;
using Zorbo.Server.WPF.Resources;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        bool forced;

        readonly AresServer server;

        SettingsWin settings;
        System.Windows.Forms.NotifyIcon icon;

        public Main()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this)) {

                var directories = new Directories() { 
                    //AppData = "Custom Data Location"
                };

                directories.EnsureExists();
                Logging.LogDirectory = directories.Logs;

                var config = Persistence.LoadModel<Configuration>(Path.Combine(directories.AppData, "config.json"));

                config.Directories = directories;
                config.PropertyChanged += Config_PropertyChanged;

                server = new AresServer(config);

                server.PluginHost = new ServerPluginHostCore(server);
                server.PropertyChanged += Server_PropertyChanged;

                DataContext = server;

                JumpList list = new JumpList {
                    ShowRecentCategory = false,
                    ShowFrequentCategory = false
                };
                /*
                list.JumpItems.Add(new JumpTask() {
                    Title = AppStrings.LabelStartServer,
                    Arguments = "-start_server",
                    ApplicationPath = App.Filename,
                    WorkingDirectory = Directories.BaseDirectory,
                });

                list.JumpItems.Add(new JumpTask() {
                    Title = AppStrings.LabelStopServer,
                    Arguments = "-stop_server",
                    ApplicationPath = App.Filename,
                    WorkingDirectory = Directories.BaseDirectory,
                });
                */
                list.JumpItems.Add(new JumpTask() {
                    Title = AppStrings.LabelLogs,
                    ApplicationPath = config.Directories.Logs,
                    CustomCategory = AppStrings.JumpCategory
                });

                list.JumpItems.Add(new JumpTask() {
                    Title = AppStrings.LabelPlugins,
                    ApplicationPath = config.Directories.Plugins,
                    CustomCategory = AppStrings.JumpCategory
                });

                JumpList.SetJumpList(System.Windows.Application.Current, list);
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
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.InvokeAsync(() => Server_PropertyChanged(sender, e));

            else if (e.PropertyName == nameof(server.Running)) {
                if (server.Running)
                    btnStart.Content = AppStrings.BtnStop;
                else {
                    btnStart.Content = AppStrings.BtnStart;
                    server.Config.SaveModel(Path.Combine(server.Config.Directories.AppData, "config.json"));
                }
            }
        }

        private bool is_resetting;
        private async void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName) {
                case nameof(Configuration.LoadWithWindows):
                    var config = (Configuration)server.Config;
                    if (config.LoadWithWindows) {
                        if (!await LaunchAdminCommand("-add_start_win")) {
                            is_resetting = true;
                            config.LoadWithWindows = false;
                        }
                    }
                    else if (!is_resetting)
                        await LaunchAdminCommand("-rem_start_win");

                    else is_resetting = false;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            icon = new System.Windows.Forms.NotifyIcon();

            System.Windows.Forms.ContextMenuStrip cm = new System.Windows.Forms.ContextMenuStrip();

            cm.Items.Add(AppStrings.LabelLogs, null, (s, e) => LaunchDirectory(server.Config.Directories.Logs));
            cm.Items.Add(AppStrings.LabelPlugins, null, (s, e) => LaunchDirectory(server.Config.Directories.Plugins));
            cm.Items.Add(new ToolStripSeparator());
            cm.Items.Add(AppStrings.LabelClose, null, (s, e) => ForceClose());

            icon.ContextMenuStrip = cm;
            icon.MouseClick += (s, args) => {
                if (args.Button == System.Windows.Forms.MouseButtons.Left) {
                    if (Visibility == Visibility.Hidden)
                        this.ForceShow();

                    else this.Hide();
                }
            };
            
            using var stream = System.Windows.Application.GetResourceStream(
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

            if (btnStart.Content.ToString() == AppStrings.BtnStart) {
                if (!server.Running)
                    server.Start();
            }
            else {
                if (server.Running)
                    server.Stop();
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e) {
            if (settings == null) {
                settings = new SettingsWin(server);
                settings.IsVisibleChanged += Settings_IsVisibleChanged;
                //settings.Owner = this;
            }
            if (settings.IsVisible)
                settings.Activate();
            else
                settings.Show();
        }

        private void Settings_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!settings.IsVisible)// "Closed"
                server.Config.SaveModel(Path.Combine(server.Config.Directories.AppData, "config.json"));
        }

        private async void LaunchDirectory(string directory) {
            try {
                await Task.Run(() => {
                    Process proc = new Process();
                    proc.StartInfo.FileName = directory;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();

                });
            }
            catch { }
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
                System.Windows.MessageBox.Show(
                    string.Format("An error occured while running an elevated command.\r\nException: {0}", ex.Message),
                    "Elevation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}
