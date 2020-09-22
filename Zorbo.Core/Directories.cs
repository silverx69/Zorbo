﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core
{
    public static class Directories
    {
        public static string AresData {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ares", "Data");
            }
        }

        public static string AppData {
            get {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zorbo");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string Logs {
            get {
                string path = Path.Combine(AppData, "Logs");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string Plugins {
            get {
                string path = Path.Combine(AppData, "Plugins");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string Cache {
            get {
                string path = Path.Combine(AppData, "Cache");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string Certificates {
            get {
                string path = Path.Combine(AppData, "Certs");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }
    }
}
