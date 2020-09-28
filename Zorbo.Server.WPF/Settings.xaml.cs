using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using Zorbo.Ares.Server;
using Zorbo.Server.WPF.Plugins;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Help Help {
            get { return (Help)base.GetValue(HelpProperty); }
        }

        public Settings(AresServer server) {

            InitializeComponent();
            DataContext = server;

            Loaded += Options_Loaded;
            Unloaded += Options_Unloaded;
        }

        private void Options_Loaded(object sender, RoutedEventArgs e) {

            AresServer server = (AresServer)DataContext;

            foreach (var plugin in server.PluginHost) {

                FrameworkElement root;
                TreeViewItem newitem = new TreeViewItem {
                    Header = plugin.Name,
                    Tag = plugin
                };

                if (plugin.Plugin is IWPFPlugin iface) {
                    if (iface.GUI == null) continue;

                    root = iface.GUI;
                }
                else if (plugin.Plugin is IWinFormsPlugin iface2) {
                    if (iface2.GUI == null) continue;

                    root = new WindowsFormsHost() { Child = iface2.GUI };
                }
                else { continue; }

                root.SetBinding(VisibilityProperty, new Binding("IsSelected") {
                    Source = newitem,
                    Converter = (BooleanToVisibilityConverter)TryFindResource("BooleanToVisibility")
                });

                Grid.SetColumn(root, 1);

                optPlugins.Items.Add(newitem);
                LayoutRoot.Children.Add(root);
            }
        }

        private void Options_Unloaded(object sender, RoutedEventArgs e) {
            AresServer server = (AresServer)DataContext;

            foreach (var plugin in server.PluginHost) {

                if (plugin.Plugin is IWPFPlugin iface) {
                    if (iface.GUI == null) continue;

                    LayoutRoot.Children.Remove(iface.GUI);
                }
                else if (plugin.Plugin is IWinFormsPlugin iface2) {
                    if (iface2.GUI == null) continue;

                    foreach (var child in LayoutRoot.Children) {
                        if (child is WindowsFormsHost host) {
                            if (host.Child == iface2.GUI) {
                                LayoutRoot.Children.Remove(host);
                                host.Child.Dispose();
                                host.Child = null;
                                host.Dispose();
                            }
                        }
                    }
                }
            }
        }


        public static readonly DependencyProperty HelpProperty =
            DependencyProperty.Register(
            "Help",
            typeof(Help),
            typeof(Settings),
            new FrameworkPropertyMetadata(new Help()));
    }
}
