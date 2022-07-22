using HunterPie.Core.Logger;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HunterPie.Internal.Logger
{
    internal class NativeLogger : ILogger
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AllocConsole();

        private enum LogLevel
        {
            Benchmark = ConsoleColor.Gray,
            Debug = ConsoleColor.DarkGreen,
            Warn = ConsoleColor.DarkYellow,
            Error = ConsoleColor.DarkRed,
            Info = ConsoleColor.DarkCyan,
            Native = ConsoleColor.Magenta
        }

        public NativeLogger()
        {
            Process currentProcess = Process.GetCurrentProcess();
            AllocConsole();
            Console.Title = "[DEBUG] HunterPie Console";
        }

        public Task Benchmark(string message) => WriteToStdout(LogLevel.Benchmark, message);
        public Task Benchmark(string format, params object[] args) => WriteToStdout(LogLevel.Benchmark, string.Format(format, args));

        public Task Debug(string message) => WriteToStdout(LogLevel.Debug, message);
        public Task Debug(string format, params object[] args) => WriteToStdout(LogLevel.Debug, string.Format(format, args));

        public Task Info(string message) => WriteToStdout(LogLevel.Info, message);
        public Task Info(string format, params object[] args) => WriteToStdout(LogLevel.Info, string.Format(format, args));

        public Task Warn(string message) => WriteToStdout(LogLevel.Warn, message);
        public Task Warn(string format, params object[] args) => WriteToStdout(LogLevel.Warn, string.Format(format, args));

        public Task Error(string message) => WriteToStdout(LogLevel.Error, message);
        public Task Error(string format, params object[] args) => WriteToStdout(LogLevel.Error, string.Format(format, args));
        
        public Task Native(string message) => WriteToStdout(LogLevel.Error, message);
        public Task Native(string format, params object[] args) => WriteToStdout(LogLevel.Native, string.Format(format, args));

        private Task WriteToStdout(LogLevel level, string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write($"[{DateTime.Now.ToLongTimeString()}]");
            Console.ForegroundColor = (ConsoleColor)level;
            Console.Write($"[{nameof(level).ToUpper()}] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);

            return Task.FromResult(true);
        }

        
    }
}
