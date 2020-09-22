using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Zorbo.Core;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;
using Zorbo.Core.Plugins;
using Zorbo.Core.Plugins.Server;

namespace Zorbo.Server.WPF.Views
{
    /// <summary>
    /// Interaction logic for Plugins.xaml
    /// </summary>
    public partial class Plugins : UserControl
    {
        readonly ObservableCollection<AvailablePlugin> files;

        public IEnumerable<AvailablePlugin> Available {
            get { return (IEnumerable<AvailablePlugin>)base.GetValue(AvailableProperty); }
        }

        public class AvailablePlugin : ModelBase
        {
            public string Name {
                get;
                private set;
            }

            public string State {
                get {
                    if (plugin == null)
                        return "Not loaded";
                    else if (!plugin.Enabled)
                        return "Disabled";
                    else
                        return "Enabled";
                }
            }

            LoadedPlugin<IServer, ServerPlugin> plugin = null;
            internal LoadedPlugin<IServer, ServerPlugin> Plugin {
                get { return plugin; }
                set {
                    if (plugin != value) {
                        plugin = value;

                        if (plugin != null)
                            plugin.PropertyChanged += Plugin_PropertyChanged;

                        OnPropertyChanged();
                    }
                }
            }


            public AvailablePlugin(string name) {
                Name = name;
            }

            private void Plugin_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                if (e.PropertyName == "Enabled") 
                    RaisePropertyChanged(nameof(State));
            }
        }

        public Plugins() {
            InitializeComponent();
            files = new ObservableCollection<AvailablePlugin>();
            base.SetValue(AvailablePropertyKey, files);
        }

        void CheckAvailable() {

            ServerPluginHost host = (ServerPluginHost)DataContext;
            foreach (var dir in new DirectoryInfo(Directories.Plugins).GetDirectories()) {

                string file = System.IO.Path.Combine(dir.FullName, dir.Name + ".dll");

                if (File.Exists(file)) {

                    var pname = files.Find((s) => s.Name == dir.Name);

                    if (pname == null) {
                        pname = new AvailablePlugin(dir.Name);
                        files.Add(pname);
                    }

                    pname.Plugin = host.Find((s) => s.Name == dir.Name);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                ServerPluginHost host = (ServerPluginHost)DataContext;
                
                host.CollectionChanged += Plugins_CollectionChanged;
                CheckAvailable();
            }
        }

        void Plugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            CheckAvailable();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e) {

        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e) 
        {
            if (!(lvAvailable.SelectedItem is AvailablePlugin p)) return;

            ServerPluginHost host = (ServerPluginHost)DataContext;

            if (p.Plugin != null && p.Plugin.Enabled) {
                host.KillPlugin(p.Name);
                btnLoad.Content = "Load";
            }
            else if (host.LoadPlugin(p.Name))
                btnLoad.Content = "Unload";

            CheckAvailable();
        }

        private void LvAvailable_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!(e.AddedItems[0] is AvailablePlugin p))
                btnLoad.IsEnabled = false;
            else {
                btnLoad.IsEnabled = true;

                if (p.Plugin != null && p.Plugin.Enabled)
                    btnLoad.Content = "Unload";
                else
                    btnLoad.Content = "Load";
            }
        }

        static readonly DependencyPropertyKey AvailablePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Available",
                typeof(IEnumerable<AvailablePlugin>),
                typeof(Plugins),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty AvailableProperty = AvailablePropertyKey.DependencyProperty;
    }
}
