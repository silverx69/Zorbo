using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using Zorbo.Ares;
using Zorbo.Core;

namespace Zorbo.Server.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0) {

                //ToDo: non-windows dependant IPC 

                if (e.Args[0] == "/start") {
                    //MainWindow.SendMessage(Win32.Commands.StartServer);
                    Shutdown();
                }
                else if (e.Args[0] == "/stop") {
                    //MainWindow.SendMessage(Win32.Commands.StopServer);
                    Shutdown();
                }

                return;
            }

            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            JumpList list = new JumpList {
                ShowRecentCategory = false,
                ShowFrequentCategory = false
            };

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string app = System.IO.Path.Combine(dir, AppDomain.CurrentDomain.FriendlyName);

            list.JumpItems.Add(new JumpTask() {
                Title = "Start server",
                Arguments = "/start",
                ApplicationPath = app,
                WorkingDirectory = dir,
            });

            list.JumpItems.Add(new JumpTask() {
                Title = "Stop server",
                Arguments = "/stop",
                ApplicationPath = app,
                WorkingDirectory = dir,
            });

            list.JumpItems.Add(new JumpTask() {
                Title = "Logs",
                ApplicationPath = Directories.Logs,
                CustomCategory = "Places"
            });

            list.JumpItems.Add(new JumpTask() {
                Title = "Plugins",
                ApplicationPath = Directories.Plugins,
                CustomCategory = "Places"
            });

            JumpList.SetJumpList(this, list);
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
