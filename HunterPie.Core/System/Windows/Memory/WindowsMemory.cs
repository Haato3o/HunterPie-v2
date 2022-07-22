using HunterPie.Core.Domain.Memory;
using HunterPie.Core.Logger;
using HunterPie.Core.System.Windows.Native;
using HunterPie.Core.Utils;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace HunterPie.Core.System.Windows.Memory
{
    public class WindowsMemory : IMemory
    {
        private const long NULLPTR = 0;

        private IntPtr pHandle;
        private ArrayPool<byte> bufferPool = ArrayPool<byte>.Shared;

        public WindowsMemory(IntPtr processHandle)
        {
            pHandle = processHandle;
        }

        public string Read(long address, uint length, Encoding encoding = null)
        {
            byte[] buffer = bufferPool.Rent((int)length);

            Kernel32.ReadProcessMemory(pHandle, (IntPtr)address, buffer, (int)length, out int _);

            string raw = (encoding ?? Encoding.UTF8).GetString(buffer, 0, (int)length);

            bufferPool.Return(buffer, true);
            int nullCharIdx = raw.IndexOf('\x00');

            if (nullCharIdx < 0)
                return raw;

            return raw[..nullCharIdx];
        }

        public T Read<T>(long address) where T : struct
        {
            return Read<T>(address, 1)[0];
        }

        public T[] Read<T>(long address, uint count) where T : struct
        {
            Type type = typeof(T);

            if (type.IsPrimitive)
                return ReadPrimitive<T>(address, count);

            return ReadStructure<T>(address, count);
        }

        public long Read(long address, int[] offsets)
        {
            foreach (int offset in offsets)
            {
                long tmp = Read<long>(address);

                if (tmp == NULLPTR)
                    return NULLPTR;

                address = tmp + offset;
            }

            return address;
        }

        public long ReadPtr(long address, int[] offsets)
        {
            foreach (int offset in offsets)
            {
                long tmp = Read<long>(address + offset);

                if (tmp == NULLPTR)
                    return NULLPTR;

                address = tmp;
            }

            return address;
        }

        private T[] ReadStructure<T>(long address, uint count) where T : struct
        {
            int size = Marshal.SizeOf<T>() * (int)count;
            IntPtr bufferAddress = Marshal.AllocHGlobal(size);
            Kernel32.ReadProcessMemory(pHandle, (IntPtr)address, bufferAddress, size, out int _);

            var structures = MarshalHelper.BufferToStructures<T>(bufferAddress, (int)count);

            Marshal.FreeHGlobal(bufferAddress);

            return structures;
        }

        private T[] ReadPrimitive<T>(long address, uint count) where T : struct
        {
            int lpByteCount = Marshal.SizeOf<T>() * (int)count;
            T[] buffer = new T[count];
            
            Kernel32.ReadProcessMemory(pHandle, (IntPtr)address, buffer, lpByteCount, out int _);

            return buffer;
        }

        public T Deref<T>(long address, int[] offsets) where T : struct
        {
            long ptr = Read(address, offsets);
            return Read<T>(ptr);
        }

        public T DerefPtr<T>(long address, int[] offsets) where T : struct
        {
            long ptr = ReadPtr(address, offsets);
            return Read<T>(ptr);
        }

        public bool Write<T>(long address, T data) where T : struct
        {
            throw new NotImplementedException();
        }

        public bool Write<T>(long address, T[] data) where T : struct
        {
            byte[] buffer = StructureToBuffer(data);

            bool result = Kernel32.WriteProcessMemory(pHandle, (IntPtr)address, buffer, buffer.Length, out int _);
            
            return result;
        }

        public bool InjectAsm(long address, byte[] asm)
        {
            Kernel32.VirtualProtectEx(pHandle, (IntPtr)address, (UIntPtr)asm.Length, 0x40, out uint oldProtect);
            bool result = Write(address, asm);
            Kernel32.VirtualProtectEx(pHandle, (IntPtr)address, (UIntPtr)asm.Length, oldProtect, out oldProtect);
            return result;
        }

        public byte[] StructureToBuffer<T>(T[] array) where T : struct
        {
            int size = Marshal.SizeOf<T>() * array.Length;
            IntPtr malloced = Marshal.AllocHGlobal(size);
            byte[] buffer = new byte[size];

            for (int i = 0; i < array.Length; i++)
            {
                int offset = i * Marshal.SizeOf<T>();
                Marshal.StructureToPtr(array[i], malloced + offset, false);
            }

            Marshal.Copy(malloced, buffer, 0, size);
            Marshal.FreeHGlobal(malloced);
            return buffer;
        }

        public bool Inject(string dll)
        {
            byte[] dllPath = Encoding.Unicode.GetBytes(dll);

            IntPtr dllNamePtr = Kernel32.VirtualAllocEx(
                pHandle,
                IntPtr.Zero,
                (uint)dllPath.Length + 1,
                Kernel32.AllocationType.Commit,
                Kernel32.MemoryProtection.ExecuteReadWrite
            );

            if (dllNamePtr == IntPtr.Zero)
                return false;
            
            Write((long)dllNamePtr, dllPath);

            Log.Debug("Wrote DLL name at {0:X}", dllNamePtr);

            IntPtr kernel32Address = Kernel32.GetModuleHandle("kernel32");
            Log.Debug("Found kernel32 address at {0:X}", kernel32Address);

            IntPtr loadLibraryW = Kernel32.GetProcAddress(kernel32Address, "LoadLibraryW");
            Log.Debug("kernel32::LoadLibraryW -> {0:X}", loadLibraryW);
            
            IntPtr lpThreadId = IntPtr.Zero;
            IntPtr thread = Kernel32.CreateRemoteThread(
                pHandle,
                IntPtr.Zero,
                0,
                loadLibraryW,
                dllNamePtr,
                0,
                lpThreadId
            );
            Log.Debug("thread {0:X}", thread);

            return thread != IntPtr.Zero;
        }
    }
}
