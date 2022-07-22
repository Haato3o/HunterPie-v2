using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ResponseInitMHHooksMessage
    {
        public HookStatus Status;
    }
}
