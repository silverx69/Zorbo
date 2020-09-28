using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zorbo.Core.Server;

namespace Zorbo.Core
{
    [Flags]
    public enum LogLevel : int
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 4,
        Debug = 8
    }

    public static class Logging
    {
        static readonly object lockObject = new object();
        static readonly DateTime InitTime = DateTime.Now;

        public static string LogDirectory { get; set; } = ".";


        public static void Error(string source, Exception ex) {
            Write(
                LogLevel.Error, 
                source, 
                "{0}: {1}{2}{3}", 
                ex.GetType().Name, 
                ex.Message,
                Environment.NewLine,
                ex.StackTrace);
        }

        public static Task ErrorAsync(string source, Exception ex) {
            return Task.Run(() => Error(source, ex));
        }

        public static void Error(string source, IClient client, Exception ex)
        {
            Write(
                LogLevel.Error,
                source,
                "{0} occured in Client '{1}' [{2}]: {3}{4}{5}",
                ex.GetType().Name,
                client.Name,
                client.ExternalIp,
                ex.Message,
                Environment.NewLine,
                ex.StackTrace);
        }

        public static Task ErrorAsync(string source, IClient client, Exception ex)
        {
            return Task.Run(() => Error(source, client, ex));
        }

        public static void Error(string source, string message, params object[] args) {
            Write(LogLevel.Error, source, message, args);
        }

        public static Task ErrorAsync(string source, string message, params object[] args) {
            return Task.Run(() => Error(source, message, args));
        }

        public static void Warning(string source, string message, params object[] args) {
            Write(LogLevel.Warning, source, message, args);
        }

        public static Task WarningAsync(string source, string message, params object[] args) {
            return Task.Run(() => Warning(source, message, args));
        }

        public static void Debug(string source, string message, params object[] args) {
            Write(LogLevel.Debug, source, message, args);
        }

        public static Task DebugAsync(string source, string message, params object[] args) {
            return Task.Run(() => Debug(source, message, args));
        }

        public static void Info(string source, string message, params object[] args) {
            Write(LogLevel.Info, source, message, args);
        }

        public static Task InfoAsync(string source, string message, params object[] args) {
            return Task.Run(() => Info(source, message, args));
        }

        public static void Write(LogLevel level, string source, string format, params object[] args) {

            string intro = string.Format("[{0}] [{1}] [{2}] ", DateTime.Now.ToString(), source, level);
            string message = string.Format(format, args);

            lock (lockObject) {

                var color = Console.ForegroundColor;

                Console.ForegroundColor = GetColorFromLevel(level);
                Console.Write(intro);
                Console.ForegroundColor = color;
                Console.WriteLine(message);

                StreamWriter writer = null;

                try {
                    writer = new StreamWriter(File.Open(GetFilename(), FileMode.Append, FileAccess.Write), Encoding.UTF8);
                    writer.Write(intro);
                    writer.WriteLine(message);
                    writer.Flush();
                }
                finally {
                    if (writer != null) {
                        writer.Close();
                        writer.Dispose();
                    }
                }
            }
        }

        public static Task WriteAsync(LogLevel level, string source, string format, params object[] args) {
            return Task.Run(() => Write(level, source, format, args));
        }

        public static void WriteLines(LogLevel level, string source, string[] lines, params object[] args)
        {
            var sb = new StringBuilder();
            foreach (var line in lines) sb.AppendLine(line);
            Write(level, source, sb.ToString(), args);
        }

        public static Task WriteLinesAsync(LogLevel level, string source, string[] lines, params object[] args)
        {
            var sb = new StringBuilder();
            foreach (var line in lines) sb.AppendLine(line);
            return WriteAsync(level, source, sb.ToString(), args);
        }

        private static string GetFilename() {
            return Path.Combine(LogDirectory, InitTime.ToString("dd-MM-yyyy") + ".txt");
        }

        private static ConsoleColor GetColorFromLevel(LogLevel level) {
            switch (level) {
                case LogLevel.Critical:
                case LogLevel.Error: return ConsoleColor.Red;
                case LogLevel.Warning: return ConsoleColor.DarkYellow;
                case LogLevel.Debug: return ConsoleColor.DarkGreen;
                case LogLevel.Info: return ConsoleColor.White;
                default: return Console.ForegroundColor;
            }
        }
    }
}
