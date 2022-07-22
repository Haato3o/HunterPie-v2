using System;

namespace HunterPie.Core.Domain.Memory
{
    public interface IMemoryWriter
    {
        public bool Write<T>(long address, T data) where T : struct;
        public bool Write<T>(long address, T[] data) where T : struct;
        public bool InjectAsm(long address, byte[] asm);
        public bool Inject(string dll);
    }
}
