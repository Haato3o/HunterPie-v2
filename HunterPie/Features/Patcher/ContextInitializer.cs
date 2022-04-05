using HunterPie.Core.Game;
using HunterPie.Domain.Interfaces;
using HunterPie.Features.Initializers;

namespace HunterPie.Features.Patcher
{
    internal static class ContextInitializer
    {
        private readonly static IContextInitializer[] initializers = new IContextInitializer[]
        {
            new RiseIntegrityPatcher(),

            new BuildExportInitializer()
        };

        public static void Initialize(Context context)
        {
            foreach (var initializer in initializers)
                initializer.Initialize(context);
        }

        public static void Unload(Context context)
        {
            foreach (var initializer in initializers)
                initializer.Unload(context);
        }
    }
}
