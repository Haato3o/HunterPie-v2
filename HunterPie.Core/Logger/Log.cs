using HunterPie.Core.Client;
using HunterPie.Core.Client.Configuration.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HunterPie.Core.Logger
{
    public class Log
    {

        private readonly List<ILogger> _io = new List<ILogger>();
        private static Log _instance;
        private readonly static SemaphoreSlim _semaphore = new(1, 1);
        private readonly Dictionary<string, Stopwatch> _benchmarkers = new Dictionary<string, Stopwatch>();
        public static Log Instance
        {
            get {
                if (_instance is null)
                {
                    _instance = new Log();
                }

                return _instance;
            }
        }

        public static async void Add(ILogger logger)
        {

            try
            {
                await _semaphore.WaitAsync();
                Instance._io.Add(logger);
            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Debug(string message)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Debug)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Debug(message);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Debug(string format, params object[] args)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Debug)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Debug(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Native(string message)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Info)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Native(message);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Native(string format, params object[] args)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Info)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Native(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Info(string message)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Info)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Info(message);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Info(string format, params object[] args)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Info)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Info(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Warn(string message)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Warn)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Warn(message);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Warn(string format, params object[] args)
        {
            if (ClientConfig.Config.Development.ClientLogLevel > LogLevel.Warn)
                return;

            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Warn(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Error(string message)
        {
            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Error(message);

            } 
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async void Error(string format, params object[] args)
        {
            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Error(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async void BenchmarkLog(string message)
        {
            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Benchmark(message);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async void BenchmarkLog(string format, params object[] args)
        {
            try
            {
                await _semaphore.WaitAsync();

                foreach (ILogger logger in Instance._io)
                    await logger.Benchmark(format, args);

            }
            catch { }
            finally
            {
                _semaphore.Release();
            }
        }

        public static void Benchmark([CallerMemberName] string name = "")
        {
            if (Instance._benchmarkers.ContainsKey(name))
                return;

            BenchmarkLog($"Starting benchmark for '{name}'");
            Instance._benchmarkers.Add(name, Stopwatch.StartNew());
        }

        public static void BenchmarkEnd([CallerMemberName] string name = "")
        {
            if (!Instance._benchmarkers.ContainsKey(name))
                return;

            Stopwatch benchmarker = Instance._benchmarkers[name];
            BenchmarkLog($"Time taken for '{name}': {benchmarker.ElapsedMilliseconds}ms");
            benchmarker.Stop();
            Instance._benchmarkers.Remove(name);
        }
    }
}
