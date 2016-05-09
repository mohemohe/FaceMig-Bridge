using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaceMig
{
    public static class NativeBridge
    {
        const string DllName = @"FaceMig.Native.dll";
        private const CharSet DefaultCharSet = CharSet.Ansi;

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void Initialize();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void Enabled();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void Disabled();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void Dispose();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void ReOpenDevice(int deviceId);

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void ModelReset();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void Track();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void ShowTrackingInfoWindow();

        [DllImport(DllName, CharSet = DefaultCharSet)]
        public static extern void CloseTrackingInfoWindow();

        [DllImport(DllName, CharSet = DefaultCharSet, EntryPoint = @"GetStatus")]
        private static unsafe extern float __GetStatus(StatusUnsafe* ptr);

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct StatusUnsafe
        {
            //total 16bytes
            public float EyeL; //4bytes
            public float EyeR; //4bytes
            public float Mouth; //4bytes
            public float leanX; //4bytes
            public float leanY; //4bytes
            public float leanZ; //4bytes
        }

        public static unsafe StatusUnsafe GetStatus()
        {
            StatusUnsafe result;
            var marshal = new StatusUnsafe[1];
            fixed (StatusUnsafe* marshalPtr = marshal)
            {
                __GetStatus(marshalPtr);
                result = *marshalPtr;
            }
            return result;
        }
    }
}
