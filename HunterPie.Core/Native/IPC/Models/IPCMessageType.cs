namespace HunterPie.Core.Native.IPC.Models
{
    public enum IPCMessageType
    {
        INIT_IPC_MEMORY_ADDRESSES,
        INIT_MH_HOOKS,
        GET_HUNT_STATISTICS,
        DELETE_HUNT_STATISTICS,
        CLEAR_HUNT_STATISTICS,
        UNKNOWN
    }
}
