using HunterPie.Core.Logger;
using HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze.Models;
using HunterPie.Core.Native.IPC.Models;
using HunterPie.Core.Native.IPC.Utils;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze
{
    internal class IPCHookInitializationMessageHandler : IMessageHandler
    {
        public int Version => 1;

        public IPCMessageType Type => IPCMessageType.INIT_MH_HOOKS;

        public void Handle(byte[] message)
        {
            ResponseInitMHHooksMessage response = MessageHelper.Deserialize<ResponseInitMHHooksMessage>(message);

            if (response.Status > HookStatus.AlreadyInitialized)
            {
                Log.Error("Failed to initialize HunterPie Native Interface hooks. Error code: {0}", response.Status);
                return;
            }

            Log.Native("Successfully initialized HunterPie Native Interface hooks!");
        }

        public static async void RequestInitMHHooks()
        {
            IPCMessage request = new IPCMessage
            {
                Type = IPCMessageType.INIT_MH_HOOKS,
                Version = 1
            };

            await IPCService.Send(request);
        }
    }
}
