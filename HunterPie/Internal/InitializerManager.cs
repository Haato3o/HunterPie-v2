using HunterPie.Core.Logger;
using HunterPie.Domain.Interfaces;
using HunterPie.Internal.Initializers;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace HunterPie.Internal
{
    internal class InitializerManager
    {
        private static HashSet<IInitializer> _initializers = new()
        {
            new CustomFontsInitializer(),
            // Core
            new LocalConfigInitializer(),
            new ClientConfigMigrationInitializer(),
            new ClientConfigInitializer(),
            new ConfigManagerInitializer(),
            new FeatureFlagsInitializer(),
            new NativeLoggerInitializer(),
            new HunterPieLoggerInitializer(),
            new MapperFactoryInitializer(),

            new ExceptionCatcherInitializer(),
            new DialogManagerInitializer(),
            new UITracerInitializer(),
            new ClientLocalizationInitializer(),
            new SystemTrayInitializer(),
            new ClientConfigBindingsInitializer(),
            // GUI
            new MenuInitializer(),

            new ApiPingInitializer(),
        };

        private static HashSet<IInitializer> _uiInitializers = new()
        {
            new HotkeyInitializer(),

            // Debugging
            new DebugWidgetInitializer(),
        };

        public static void Initialize()
        {
            Log.Benchmark();
            
            foreach (IInitializer initializer in _initializers)
                initializer.Init();

            Log.BenchmarkEnd();
        } 

        public static void InitializeGUI()
        {
            Log.Benchmark();

            // Make sure to run UI initializers in the main thread
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                foreach (IInitializer initializer in _uiInitializers)
                    initializer.Init();
            });          

            Log.BenchmarkEnd();
        }

        public static void Unload()
        {
            foreach (IInitializer initializer in _initializers)
                if (initializer is IDisposable disposable)
                    disposable.Dispose();

            foreach (IInitializer initializer in _uiInitializers)
                if (initializer is IDisposable disposable)
                    disposable.Dispose();
        }
    }
}
