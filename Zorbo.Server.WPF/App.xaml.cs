using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using Zorbo.Core;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static readonly string RunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static readonly string Filename = Path.Combine(
            Directories.BaseDirectory, 
            $"{AppDomain.CurrentDomain.FriendlyName}.exe");

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0) {
                //ToDo: non-windows dependant IPC 
                try {

                    switch (e.Args[0]) {
                        case "-start_server":
                            
                            break;
                        case "-stop_server":

                            break;
                        case "-add_start_win": {
                            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunPath, true);
                            key.SetValue("Zorbo", Filename, RegistryValueKind.String);
                            break;
                        }
                        case "-rem_start_win": {
                            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunPath, true);
                            key.DeleteValue("Zorbo", false);
                            break;
                        }
                    }
                }
                catch {
                    Application.Current.Shutdown(-1);
                }

                Application.Current.Shutdown(0);
            }
            else {

                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            }
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.Error("Unhandled Exception", (Exception)e.ExceptionObject);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
