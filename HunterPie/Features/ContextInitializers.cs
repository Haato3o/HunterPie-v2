using HunterPie.Core.Game;
using HunterPie.Domain.Interfaces;
using HunterPie.Features.Native;

namespace HunterPie.Features
{
    internal static class ContextInitializers
    {
        private static IContextInitializer[] initializers = new IContextInitializer[]
        {
            new IPCInjectorInitializer(),
            new NativeIPCInitializer(),
        };

        public static void Initialize(Context context)
        {
            foreach (var initializer in initializers)
                initializer.Initialize(context);
        }
    }
}
