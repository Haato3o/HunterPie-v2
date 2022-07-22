using HunterPie.Core.Address.Map;
using HunterPie.Core.Game;
using HunterPie.Core.Game.Rise;
using HunterPie.Core.Logger;
using HunterPie.Core.Native.IPC;
using HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze;
using HunterPie.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HunterPie.Features.Native
{
    internal class NativeIPCInitializer : IContextInitializer
    {

        private static readonly string[] _addresses = new[]
        {
            "FUN_CALCULATE_ENTITY_DAMAGE"
        };

        public void Initialize(Context context)
        {
            if (context is not MHRContext)
                return;

            IPCService.Initialize().ContinueWith(async (success) =>
            {
                bool connected = await success;
                
                if (!connected)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        Log.Debug($"Retrying to connect: Attempt {i}...");

                        connected = await IPCService.Initialize();

                        if (connected)
                            break;

                        await Task.Delay(i * 100);
                    }
                }

                UIntPtr[] addresses = GetAddressesToHook();

                IPCInitializationMessageHandler.RequestIPCInitialization(addresses);
            });
        }

        private UIntPtr[] GetAddressesToHook()
        {
            return _addresses.Select(name => (UIntPtr)AddressMap.GetAbsolute(name))
                             .ToArray();
        }
    }
}
