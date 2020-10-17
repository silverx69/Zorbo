﻿using System;
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
        static readonly object LockObj = new object();
        static readonly DateTime InitTime = DateTime.Now;

        public static LogLevel LogLevel { get; set; } = LogLevel.Info;
        public static string LogDirectory { get; set; } = ".";

        public static void Critical(string source, Exception ex)
        {
            Write(LogLevel.Critical, source, "{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);
        }

        public static void Error(string source, Exception ex)
        {
            if (LogLevel >= LogLevel.Debug)
                Write(LogLevel.Error, source, "{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);
            else
                Write(LogLevel.Error, source, "{0}: {1}", ex.GetType().Name, ex.Message);
        }

        public static void Error(string source, ISocket socket, Exception ex)
        {
            if (socket == null)
                Error(source, ex);
            else {
                if (LogLevel >= LogLevel.Debug)
                    Write(
                        LogLevel.Error, source, "{0} occured in TLS client '{1}': {2}{3}{4}",
                        ex.GetType().Name, socket.RemoteEndPoint.Address, ex.Message, Environment.NewLine, ex.StackTrace);
                else
                    Write(
                        LogLevel.Error, source, "{0} occured in TLS client '{1}': {2}",
                        ex.GetType().Name, socket.RemoteEndPoint.Address, ex.Message);
            }
        }

        public static void Error(string source, IClient client, Exception ex)
        {
            if (client == null)
                Error(source, ex);
            else {
                if (LogLevel >= LogLevel.Debug)
                    Write(
                        LogLevel.Error, source, "{0} occured in Client '{1}' [{2}]: {3}{4}{5}",
                        ex.GetType().Name, client.Name, client.ExternalIp, ex.Message, Environment.NewLine, ex.StackTrace);
                else
                    Write(
                        LogLevel.Error, source, "{0} occured in Client '{1}' [{2}]: {3}",
                        ex.GetType().Name, client.Name, client.ExternalIp, ex.Message);
            }
        }

        public static void Error(string source, string message, params object[] args)
        {
            Write(LogLevel.Error, source, message, args);
        }

        public static void Warning(string source, string message, params object[] args)
        {
            Write(LogLevel.Warning, source, message, args);
        }

        public static void Debug(string source, string message, params object[] args)
        {
            Write(LogLevel.Debug, source, message, args);
        }

        public static void Info(string source, string message, params object[] args)
        {
            Write(LogLevel.Info, source, message, args);
        }

        public static void Write(LogLevel level, string source, string format, params object[] args)
        {
            if (level <= LogLevel) {
                string intro = string.Format("[{0}] [{1}] [{2}] ", DateTime.Now.ToString(), source, level);
                string message = string.Format(format, args);
#if DEBUG
                static ConsoleColor GetColorFromLevel(LogLevel level)
                {
                    switch (level) {
                        case LogLevel.Critical:
                        case LogLevel.Error: return ConsoleColor.Red;
                        case LogLevel.Warning: return ConsoleColor.DarkYellow;
                        case LogLevel.Debug: return ConsoleColor.DarkGreen;
                        case LogLevel.Info: return ConsoleColor.White;
                        default: return Console.ForegroundColor;
                    }
                }
                var color = Console.ForegroundColor;
                Console.ForegroundColor = GetColorFromLevel(level);
                Console.Write(intro);
                System.Diagnostics.Debug.Write(intro);
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                System.Diagnostics.Debug.Write(message);
#endif
                lock (LockObj) {
                    using var writer = GetWriter();
                    writer.WriteLine(intro + message);
                    writer.Flush();
                }
            }
        }

        public static void WriteLines(LogLevel level, string source, string[] lines, params object[] args)
        {
            if (level <= LogLevel) {
                string intro = string.Format("[{0}] [{1}] [{2}] ", DateTime.Now.ToString(), source, level);

                var sb = new StringBuilder();
                foreach (var line in lines) sb.AppendLine(intro + line);

                lock (LockObj) {
                    using var writer = GetWriter();

                    writer.Write(string.Format(sb.ToString(), args));
                    writer.Flush();
                }
            }
        }

        private static StreamWriter GetWriter() 
        { 
            return new StreamWriter(File.Open(GetFilename(), FileMode.Append, FileAccess.Write), Encoding.UTF8);
        }

        private static string GetFilename()
        {
            return Path.Combine(LogDirectory, InitTime.ToString("dd-MM-yyyy") + ".txt");
        }
    }
}
