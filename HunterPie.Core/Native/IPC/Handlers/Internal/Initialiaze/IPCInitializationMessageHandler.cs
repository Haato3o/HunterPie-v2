using HunterPie.Core.Logger;
using HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze.Models;
using HunterPie.Core.Native.IPC.Models;
using HunterPie.Core.Native.IPC.Utils;
using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze
{
    internal class IPCInitializationMessageHandler : IMessageHandler
    {
        const string ERROR_DIALOG_MESSAGE = "HunterPie has detected wrong version of HunterPie Native Interface currently in the game.\nYou must restart your game for it to work properly";
        public int Version => 2;

        public IPCMessageType Type => IPCMessageType.INIT_IPC_MEMORY_ADDRESSES;
                
        public void Handle(byte[] message)
        {
            IPCMessage response = MessageHelper.Deserialize<IPCMessage>(message);

            if (response.Version != Version)
            {
                Log.Warn(ERROR_DIALOG_MESSAGE);
                return;
            }

            IPCHookInitializationMessageHandler.RequestInitMHHooks();
        }

        public static async void RequestIPCInitialization(UIntPtr[] addresses)
        {
            UIntPtr[] buffer = new UIntPtr[256];

            Buffer.BlockCopy(addresses, 0, buffer, 0, addresses.Length * Marshal.SizeOf<UIntPtr>());

            RequestIPCInitializationMessage request = new RequestIPCInitializationMessage()
            {
                Header = new IPCMessage
                {
                    Type = IPCMessageType.INIT_IPC_MEMORY_ADDRESSES,
                    Version = 1,
                },
                Addresses = buffer,
            };

            await IPCService.Send(request);
        }

        
    }
}
