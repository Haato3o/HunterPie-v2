using HunterPie.Core.Logger;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace HunterPie.UI.Logger
{
    public class HunterPieLogger : ILogger
    {

        public static readonly ObservableCollection<LogString> ViewModel = new ObservableCollection<LogString>();

        public Task Benchmark(string message) => WriteToBuffer(LogLevel.Benchmark, message);
        public Task Benchmark(string format, params object[] args) => WriteToBuffer(LogLevel.Benchmark, string.Format(format, args));

        public Task Debug(string message) => WriteToBuffer(LogLevel.Debug, message);
        public Task Debug(string format, params object[] args) => WriteToBuffer(LogLevel.Debug, string.Format(format, args));

        public Task Native(string message) => WriteToBuffer(LogLevel.Native, message);
        public Task Native(string format, params object[] args) => WriteToBuffer(LogLevel.Native, string.Format(format, args));

        public Task Info(string message) => WriteToBuffer(LogLevel.Info, message);
        public Task Info(string format, params object[] args) => WriteToBuffer(LogLevel.Info, string.Format(format, args));

        public Task Warn(string message) => WriteToBuffer(LogLevel.Warn, message);
        public Task Warn(string format, params object[] args) => WriteToBuffer(LogLevel.Warn, string.Format(format, args));

        public Task Error(string message) => WriteToBuffer(LogLevel.Error, message);
        public Task Error(string format, params object[] args) => WriteToBuffer(LogLevel.Error, string.Format(format, args));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task WriteToBuffer(LogLevel level, string message)
        {
            if (Application.Current is null)
                return;
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ViewModel.Add(new LogString
                {
                    Timestamp = $"[{DateTime.Now.ToLongTimeString()}]",
                    Message = message,
                    Level = level
                });
            });
        }
    }
}
