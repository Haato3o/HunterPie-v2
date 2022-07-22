namespace HunterPie.Core.Native.IPC.Handlers.Internal.Initialiaze.Models
{
    public enum HookStatus : int
    {
        Unknown = -1,
        Ok,
        AlreadyInitialized,
        NotInitialized,
        AlreadyCreated,
        NotCreated,
        Enabled,
        Disabled,
        NotExecutable,
        UnsupportedFunction,
        MemoryAlloc,
        ModuleNotFound,
        FunctionNotFound
    }
}
