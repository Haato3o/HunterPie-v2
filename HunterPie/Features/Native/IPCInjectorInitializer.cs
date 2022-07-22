using HunterPie.Core.Client;
using HunterPie.Core.Game;
using HunterPie.Core.Game.Rise;
using HunterPie.Core.Logger;
using HunterPie.Domain.Interfaces;
using System.Diagnostics;
using System.Linq;

namespace HunterPie.Features.Native
{
    internal class IPCInjectorInitializer : IContextInitializer
    {
        const string NATIVE_NAME = "HunterPie.Native.dll";
        const string NATIVE_PATH = "libs/" + NATIVE_NAME;

        public void Initialize(Context context)
        {
            if (context is not MHRContext)
                return;

            string native = ClientInfo.GetPathFor(NATIVE_PATH);
            
            if (IsAlreadyInjected(context))
            {
                Log.Native("HunterPie Native Interface is already running");
                return;
            }

            if (!context.Process.Memory.Inject(native))
            {
                Log.Error("Failed to inject HunterPie Native Interface");
                return;
            }

            Log.Native("HunterPie Native Interface injected successfully!");
        }

        private static bool IsAlreadyInjected(Context context)
        {
            return context.Process.Process.Modules.Cast<ProcessModule>()
                .Any(module => module.ModuleName == NATIVE_NAME);
        }
    }
}
