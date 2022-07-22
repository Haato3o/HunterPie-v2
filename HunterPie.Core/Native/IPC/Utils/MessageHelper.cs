using System;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Utils
{
    public class MessageHelper
    {
        public static byte[] Serialize<T>(T data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr gAlloc = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(data, gAlloc, true);

            byte[] buffer = new byte[size];
            Marshal.Copy(gAlloc, buffer, 0, buffer.Length);

            Marshal.FreeHGlobal(gAlloc);
            return buffer;
        }

        public static T Deserialize<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr mAlloc = Marshal.AllocHGlobal(size);

            Marshal.Copy(buffer, 0, mAlloc, size);

            T deserialized = Marshal.PtrToStructure<T>(mAlloc);

            Marshal.FreeHGlobal(mAlloc);

            return deserialized;
        }
    }
}
