using HunterPie.Core.Native.IPC.Models;

namespace HunterPie.Core.Native.IPC.Handlers
{
    public interface IMessageHandler
    {
        public int Version { get; }
        public IPCMessageType Type { get; }

        public void Handle(byte[] message);
    }
}
